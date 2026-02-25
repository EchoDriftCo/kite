using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeVault.Data;
using RecipeVault.Domain.Entities;
using RecipeVault.Integrations.Usda;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.DomainService {
    public class NutritionService : INutritionService {
        private readonly IRecipeVaultDbContext _context;
        private readonly IUsdaFoodDataService _usdaService;
        private readonly IIngredientParser _ingredientParser;
        private readonly IUnitConverter _unitConverter;
        private readonly ILogger<NutritionService> _logger;

        public NutritionService(
            IRecipeVaultDbContext context,
            IUsdaFoodDataService usdaService,
            IIngredientParser ingredientParser,
            IUnitConverter unitConverter,
            ILogger<NutritionService> logger) {
            _context = context;
            _usdaService = usdaService;
            _ingredientParser = ingredientParser;
            _unitConverter = unitConverter;
            _logger = logger;
        }

        public async Task<RecipeNutrition> AnalyzeRecipeNutritionAsync(int recipeId) {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);

            if (recipe == null) {
                throw new InvalidOperationException($"Recipe {recipeId} not found");
            }

            // Clear existing ingredient nutrition data
            var existingIngredientNutrition = await _context.IngredientNutritions
                .Where(i_n => recipe.Ingredients.Select(ing => ing.RecipeIngredientId).Contains(i_n.RecipeIngredientId))
                .ToListAsync();
            _context.IngredientNutritions.RemoveRange(existingIngredientNutrition);

            // Analyze each ingredient
            int matched = 0;
            decimal totalCalories = 0m;
            decimal totalProtein = 0m;
            decimal totalCarbs = 0m;
            decimal totalFat = 0m;
            decimal totalFiber = 0m;
            decimal totalSugar = 0m;
            decimal totalSodium = 0m;

            foreach (var ingredient in recipe.Ingredients) {
                try {
                    var parsed = _ingredientParser.Parse(ingredient.Text);
                    if (string.IsNullOrWhiteSpace(parsed.Item)) {
                        _logger.LogWarning("Could not parse ingredient: {Text}", ingredient.Text);
                        continue;
                    }

                    // Search USDA for the ingredient
                    var searchResults = await _usdaService.SearchWithBestMatchAsync(parsed.Item, 3);
                    if (!searchResults.Any()) {
                        _logger.LogWarning("No USDA match found for: {Item}", parsed.Item);
                        continue;
                    }

                    var bestMatch = searchResults.First();
                    var confidence = CalculateMatchConfidence(parsed.Item, bestMatch.Description);

                    // Convert to grams
                    var quantity = parsed.Quantity ?? 1m;
                    var grams = await _unitConverter.ConvertToGramsAsync(quantity, parsed.Unit, parsed.Item, bestMatch.FdcId);
                    if (!grams.HasValue) {
                        _logger.LogWarning("Could not convert unit {Unit} to grams for {Item}", parsed.Unit, parsed.Item);
                        continue;
                    }

                    // Extract nutrition per 100g from USDA data
                    var nutrientsPer100g = ExtractNutrients(bestMatch);

                    // Calculate nutrition for the actual amount
                    var scaleFactor = grams.Value / 100m;
                    var ingredientNutrition = new IngredientNutrition(
                        ingredient.RecipeIngredientId,
                        bestMatch.FdcId,
                        bestMatch.Description,
                        confidence,
                        grams.Value
                    );

                    ingredientNutrition.SetNutritionValues(
                        nutrientsPer100g.Calories * scaleFactor,
                        nutrientsPer100g.Protein * scaleFactor,
                        nutrientsPer100g.Carbs * scaleFactor,
                        nutrientsPer100g.Fat * scaleFactor,
                        nutrientsPer100g.Fiber * scaleFactor,
                        nutrientsPer100g.Sugar * scaleFactor,
                        nutrientsPer100g.Sodium * scaleFactor
                    );

                    _context.IngredientNutritions.Add(ingredientNutrition);

                    // Add to totals
                    totalCalories += ingredientNutrition.Calories ?? 0m;
                    totalProtein += ingredientNutrition.ProteinGrams ?? 0m;
                    totalCarbs += ingredientNutrition.CarbsGrams ?? 0m;
                    totalFat += ingredientNutrition.FatGrams ?? 0m;
                    totalFiber += ingredientNutrition.FiberGrams ?? 0m;
                    totalSugar += ingredientNutrition.SugarGrams ?? 0m;
                    totalSodium += ingredientNutrition.SodiumMg ?? 0m;

                    matched++;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error analyzing ingredient: {Text}", ingredient.Text);
                }
            }

            // Calculate per-serving values
            var servings = recipe.Servings > 0 ? recipe.Servings : 1;

            // Create or update recipe nutrition
            var recipeNutrition = await _context.RecipeNutritions
                .FirstOrDefaultAsync(rn => rn.RecipeId == recipeId);

            if (recipeNutrition == null) {
                recipeNutrition = new RecipeNutrition(recipeId);
                _context.RecipeNutritions.Add(recipeNutrition);
            }

            recipeNutrition.SetPerServingValues(
                Math.Round(totalCalories / servings, 1),
                Math.Round(totalProtein / servings, 1),
                Math.Round(totalCarbs / servings, 1),
                Math.Round(totalFat / servings, 1),
                Math.Round(totalFiber / servings, 1),
                Math.Round(totalSugar / servings, 1),
                Math.Round(totalSodium / servings, 1)
            );

            recipeNutrition.SetCoverageMetrics(matched, recipe.Ingredients.Count);
            recipeNutrition.MarkAsFresh();

            await _context.SaveChangesAsync();

            return recipeNutrition;
        }

        public async Task<RecipeNutrition> GetRecipeNutritionAsync(int recipeId) {
            return await _context.RecipeNutritions
                .Include(rn => rn.Recipe)
                .FirstOrDefaultAsync(rn => rn.RecipeId == recipeId);
        }

        public async Task<IngredientNutrition> UpdateIngredientNutritionAsync(int recipeIngredientId, int? fdcId, string matchedFoodName, decimal matchConfidence) {
            var ingredient = await _context.RecipeIngredients
                .FirstOrDefaultAsync(ri => ri.RecipeIngredientId == recipeIngredientId);

            if (ingredient == null) {
                throw new InvalidOperationException($"Ingredient {recipeIngredientId} not found");
            }

            // Find or create ingredient nutrition
            var ingredientNutrition = await _context.IngredientNutritions
                .FirstOrDefaultAsync(i_n => i_n.RecipeIngredientId == recipeIngredientId);

            if (ingredientNutrition == null) {
                var parsed = _ingredientParser.Parse(ingredient.Text);
                var quantity = parsed.Quantity ?? 1m;
                var grams = await _unitConverter.ConvertToGramsAsync(quantity, parsed.Unit, parsed.Item, fdcId);

                ingredientNutrition = new IngredientNutrition(
                    recipeIngredientId,
                    fdcId,
                    matchedFoodName,
                    matchConfidence,
                    grams ?? 100m
                );
                _context.IngredientNutritions.Add(ingredientNutrition);
            }

            // Fetch nutrition data from USDA if FdcId provided
            if (fdcId.HasValue) {
                var searchResults = await _usdaService.SearchFoodsAsync(matchedFoodName, 1);
                if (searchResults.Foods.Any()) {
                    var food = searchResults.Foods.First();
                    var nutrients = ExtractNutrients(food);
                    var scaleFactor = ingredientNutrition.GramsUsed / 100m;

                    ingredientNutrition.SetNutritionValues(
                        nutrients.Calories * scaleFactor,
                        nutrients.Protein * scaleFactor,
                        nutrients.Carbs * scaleFactor,
                        nutrients.Fat * scaleFactor,
                        nutrients.Fiber * scaleFactor,
                        nutrients.Sugar * scaleFactor,
                        nutrients.Sodium * scaleFactor
                    );
                }
            }

            ingredientNutrition.MarkAsManualOverride();
            await _context.SaveChangesAsync();

            // Mark recipe nutrition as stale
            var recipeNutrition = await _context.RecipeNutritions
                .FirstOrDefaultAsync(rn => rn.RecipeId == ingredient.RecipeId);
            if (recipeNutrition != null) {
                recipeNutrition.MarkAsStale();
                await _context.SaveChangesAsync();
            }

            return ingredientNutrition;
        }

        public async Task<FoodSearchResponse> SearchFoodsAsync(string query) {
            return await _usdaService.SearchFoodsAsync(query);
        }

        private decimal CalculateMatchConfidence(string searchTerm, string matchedName) {
            if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(matchedName)) {
                return 0m;
            }

            var searchLower = searchTerm.ToLower();
            var matchLower = matchedName.ToLower();

            // Exact match
            if (searchLower == matchLower) return 1.0m;

            // Contains search term
            if (matchLower.Contains(searchLower)) return 0.8m;

            // Word overlap
            var searchWords = searchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var matchWords = matchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var overlap = searchWords.Intersect(matchWords).Count();
            var maxWords = Math.Max(searchWords.Length, matchWords.Length);

            if (maxWords == 0) return 0.5m;

            return Math.Max(0.5m, (decimal)overlap / maxWords);
        }

        private (decimal Calories, decimal Protein, decimal Carbs, decimal Fat, decimal Fiber, decimal Sugar, decimal Sodium) ExtractNutrients(FoodSearchResult food) {
            decimal GetNutrientValue(string nutrientNumber) {
                var nutrient = food.FoodNutrients?.FirstOrDefault(n => n.NutrientNumber == nutrientNumber);
                return nutrient?.Value ?? 0m;
            }

            return (
                Calories: GetNutrientValue("208"),
                Protein: GetNutrientValue("203"),
                Carbs: GetNutrientValue("205"),
                Fat: GetNutrientValue("204"),
                Fiber: GetNutrientValue("291"),
                Sugar: GetNutrientValue("269"),
                Sodium: GetNutrientValue("307")
            );
        }
    }
}
