using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public class UnitConverter : IUnitConverter {
        private readonly IUsdaFoodDataService _usdaService;
        private readonly ILogger<UnitConverter> _logger;

        // Weight conversions (direct to grams)
        private static readonly Dictionary<string, decimal> WeightToGrams = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) {
            { "g", 1m },
            { "gram", 1m },
            { "grams", 1m },
            { "kg", 1000m },
            { "kilogram", 1000m },
            { "kilograms", 1000m },
            { "mg", 0.001m },
            { "milligram", 0.001m },
            { "milligrams", 0.001m },
            { "oz", 28.35m },
            { "ounce", 28.35m },
            { "ounces", 28.35m },
            { "lb", 453.592m },
            { "lbs", 453.592m },
            { "pound", 453.592m },
            { "pounds", 453.592m }
        };

        // Volume to grams (using water density as baseline, can be adjusted per ingredient)
        private static readonly Dictionary<string, decimal> VolumeToMl = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) {
            { "ml", 1m },
            { "milliliter", 1m },
            { "milliliters", 1m },
            { "l", 1000m },
            { "liter", 1000m },
            { "liters", 1000m },
            { "tsp", 4.929m },
            { "teaspoon", 4.929m },
            { "teaspoons", 4.929m },
            { "tbsp", 14.787m },
            { "tablespoon", 14.787m },
            { "tablespoons", 14.787m },
            { "fl oz", 29.574m },
            { "fluid ounce", 29.574m },
            { "fluid ounces", 29.574m },
            { "cup", 236.588m },
            { "cups", 236.588m },
            { "pint", 473.176m },
            { "pints", 473.176m },
            { "quart", 946.353m },
            { "quarts", 946.353m },
            { "gallon", 3785.41m },
            { "gallons", 3785.41m }
        };

        // Density estimates for common ingredients (g/ml)
        private static readonly Dictionary<string, decimal> IngredientDensities = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) {
            // Liquids
            { "water", 1.0m },
            { "milk", 1.03m },
            { "oil", 0.92m },
            { "olive oil", 0.92m },
            { "vegetable oil", 0.92m },
            { "vinegar", 1.01m },
            { "honey", 1.42m },
            { "syrup", 1.37m },
            { "broth", 1.0m },
            { "stock", 1.0m },
            { "juice", 1.04m },
            // Dry goods
            { "flour", 0.59m },
            { "sugar", 0.85m },
            { "brown sugar", 0.85m },
            { "salt", 1.22m },
            { "rice", 0.85m },
            { "oats", 0.41m },
            { "butter", 0.96m },
            // Paste/semi-solid
            { "peanut butter", 1.08m },
            { "yogurt", 1.04m },
            { "sour cream", 1.02m },
            { "tomato paste", 1.17m }
        };

        public UnitConverter(IUsdaFoodDataService usdaService, ILogger<UnitConverter> logger) {
            _usdaService = usdaService;
            _logger = logger;
        }

        public async Task<decimal?> ConvertToGramsAsync(decimal quantity, string unit, string ingredientName, int? fdcId = null) {
            if (string.IsNullOrWhiteSpace(unit)) {
                // No unit specified, assume grams
                return quantity;
            }

            // Direct weight conversion
            if (WeightToGrams.TryGetValue(unit, out var gramsPerUnit)) {
                return quantity * gramsPerUnit;
            }

            // Volume conversion (needs density)
            if (VolumeToMl.TryGetValue(unit, out var mlPerUnit)) {
                var ml = quantity * mlPerUnit;
                
                // Try to get density from USDA portions if FdcId is provided
                if (fdcId.HasValue) {
                    var density = await GetDensityFromUsdaAsync(fdcId.Value, unit);
                    if (density.HasValue) {
                        return ml * density.Value;
                    }
                }

                // Fall back to ingredient-based density estimate
                var estimatedDensity = EstimateDensity(ingredientName);
                return ml * estimatedDensity;
            }

            // Count-based items (pieces, cloves, etc.)
            if (IsCountUnit(unit)) {
                // For count units, try to get average weight from USDA
                if (fdcId.HasValue) {
                    var averageWeight = await GetAverageWeightFromUsdaAsync(fdcId.Value);
                    if (averageWeight.HasValue) {
                        return quantity * averageWeight.Value;
                    }
                }

                // Fall back to rough estimates for common count items
                var estimatedGrams = EstimateCountWeight(unit, ingredientName);
                return quantity * estimatedGrams;
            }

            _logger.LogWarning("Unknown unit: {Unit} for ingredient: {Ingredient}", unit, ingredientName);
            return null;
        }

        private async Task<decimal?> GetDensityFromUsdaAsync(int fdcId, string unit) {
            try {
                var details = await _usdaService.GetFoodDetailsAsync(fdcId);
                if (details?.FoodPortions != null) {
                    // Look for a portion that matches the unit
                    var matchingPortion = details.FoodPortions.FirstOrDefault(p =>
                        p.MeasureUnit?.Name?.Equals(unit, StringComparison.OrdinalIgnoreCase) == true ||
                        p.MeasureUnit?.Abbreviation?.Equals(unit, StringComparison.OrdinalIgnoreCase) == true);

                    if (matchingPortion != null && matchingPortion.Amount > 0) {
                        // Calculate grams per unit based on USDA portion data
                        return matchingPortion.GramWeight / matchingPortion.Amount;
                    }
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Error fetching USDA density for FdcId {FdcId}", fdcId);
            }

            return null;
        }

        private async Task<decimal?> GetAverageWeightFromUsdaAsync(int fdcId) {
            try {
                var details = await _usdaService.GetFoodDetailsAsync(fdcId);
                if (details?.FoodPortions != null && details.FoodPortions.Any()) {
                    // Use the first portion as a reasonable estimate
                    var portion = details.FoodPortions.FirstOrDefault();
                    if (portion != null && portion.Amount > 0) {
                        return portion.GramWeight / portion.Amount;
                    }
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Error fetching USDA weight for FdcId {FdcId}", fdcId);
            }

            return null;
        }

        private decimal EstimateDensity(string ingredientName) {
            var lowerName = ingredientName.ToLower();

            // Check for exact matches first
            foreach (var kvp in IngredientDensities) {
                if (lowerName.Contains(kvp.Key.ToLower())) {
                    return kvp.Value;
                }
            }

            // Default to water density
            return 1.0m;
        }

        private bool IsCountUnit(string unit) {
            var countUnits = new[] { "piece", "pieces", "whole", "clove", "cloves", "slice", "slices", 
                "can", "cans", "package", "packages", "box", "boxes", "bunch", "bunches" };
            return countUnits.Contains(unit.ToLower());
        }

        private decimal EstimateCountWeight(string unit, string ingredientName) {
            var lowerName = ingredientName.ToLower();
            var lowerUnit = unit.ToLower();

            // Cloves (garlic, etc.)
            if (lowerUnit.Contains("clove")) {
                return 5m; // ~5g per clove of garlic
            }

            // Slices
            if (lowerUnit.Contains("slice")) {
                if (lowerName.Contains("bread")) return 30m;
                if (lowerName.Contains("cheese")) return 20m;
                return 25m; // default
            }

            // Eggs
            if (lowerName.Contains("egg")) {
                return 50m; // ~50g per large egg
            }

            // Cans/packages - very rough
            if (lowerUnit.Contains("can")) return 400m;
            if (lowerUnit.Contains("package")) return 200m;

            // Default piece weight
            return 100m;
        }
    }
}
