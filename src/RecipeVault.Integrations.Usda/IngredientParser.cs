using System;
using System.Linq;
using System.Text.RegularExpressions;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public class IngredientParser : IIngredientParser {
        private static readonly string[] CommonUnits = new[] {
            // Volume
            "cup", "cups", "tablespoon", "tablespoons", "tbsp", "tsp", "teaspoon", "teaspoons",
            "fluid ounce", "fluid ounces", "fl oz", "pint", "pints", "quart", "quarts",
            "gallon", "gallons", "milliliter", "milliliters", "ml", "liter", "liters", "l",
            // Weight
            "pound", "pounds", "lb", "lbs", "ounce", "ounces", "oz",
            "gram", "grams", "g", "kilogram", "kilograms", "kg", "mg", "milligram", "milligrams",
            // Count/Other
            "piece", "pieces", "whole", "clove", "cloves", "slice", "slices",
            "can", "cans", "package", "packages", "box", "boxes", "bunch", "bunches",
            "pinch", "pinches", "dash", "dashes", "handful", "handfuls"
        };

        private static readonly string[] PreparationWords = new[] {
            "chopped", "diced", "sliced", "minced", "crushed", "grated", "shredded",
            "peeled", "trimmed", "halved", "quartered", "cubed", "julienned",
            "melted", "softened", "beaten", "whisked", "cooked", "roasted",
            "finely", "roughly", "coarsely", "thinly", "thickly"
        };

        public ParsedIngredient Parse(string ingredientText) {
            if (string.IsNullOrWhiteSpace(ingredientText)) {
                return new ParsedIngredient { OriginalText = ingredientText };
            }

            var result = new ParsedIngredient {
                OriginalText = ingredientText
            };

            var text = ingredientText.Trim();

            // Extract quantity (number at the beginning, possibly with fractions)
            var quantityMatch = Regex.Match(text, @"^(\d+(?:\.\d+)?(?:\s*/\s*\d+)?(?:\s+\d+/\d+)?)\s+");
            if (quantityMatch.Success) {
                result.Quantity = ParseQuantity(quantityMatch.Groups[1].Value);
                text = text.Substring(quantityMatch.Length).TrimStart();
            }

            // Extract unit
            var unitPattern = string.Join("|", CommonUnits.OrderByDescending(u => u.Length).Select(Regex.Escape));
            var unitMatch = Regex.Match(text, $@"^({unitPattern})\b", RegexOptions.IgnoreCase);
            if (unitMatch.Success) {
                result.Unit = unitMatch.Groups[1].Value.ToLower();
                text = text.Substring(unitMatch.Length).TrimStart();
            }

            // Extract preparation (words in parentheses or common prep words at the end)
            var parenMatch = Regex.Match(text, @"\(([^)]+)\)");
            if (parenMatch.Success) {
                result.Preparation = parenMatch.Groups[1].Value.Trim();
                text = text.Replace(parenMatch.Value, "").Trim();
            } else {
                // Check for preparation words at the end
                var prepPattern = string.Join("|", PreparationWords.Select(Regex.Escape));
                var prepMatch = Regex.Match(text, $@",?\s+({prepPattern}(?:\s+{prepPattern})*)\s*$", RegexOptions.IgnoreCase);
                if (prepMatch.Success) {
                    result.Preparation = prepMatch.Groups[1].Value.Trim();
                    text = text.Substring(0, text.Length - prepMatch.Length).Trim();
                }
            }

            // Remaining text is the item
            result.Item = text.Trim().TrimEnd(',', ';');

            return result;
        }

        private decimal ParseQuantity(string quantityText) {
            quantityText = quantityText.Replace(" ", "").Trim();

            // Handle fractions like "1/2" or "1 1/2"
            var fractionMatch = Regex.Match(quantityText, @"^(\d+)?/?(\d+)/(\d+)$");
            if (fractionMatch.Success) {
                decimal whole = 0;
                if (!string.IsNullOrEmpty(fractionMatch.Groups[1].Value)) {
                    whole = decimal.Parse(fractionMatch.Groups[1].Value);
                }
                var numerator = decimal.Parse(fractionMatch.Groups[2].Value);
                var denominator = decimal.Parse(fractionMatch.Groups[3].Value);
                return whole + (numerator / denominator);
            }

            // Simple decimal
            if (decimal.TryParse(quantityText, out var result)) {
                return result;
            }

            return 1;
        }
    }
}
