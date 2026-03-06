using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RecipeVault.WebApi.Services {
    public static class GroceryCheckoutUrlBuilder {
        private static readonly string[] SupportedServices = { "instacart", "walmart", "amazonfresh", "shipt", "manual" };

        public static IReadOnlyList<string> GetSupportedServices() => SupportedServices;

        public static bool IsSupportedService(string service) {
            if (string.IsNullOrWhiteSpace(service)) {
                return false;
            }

            return SupportedServices.Contains(service.Trim().ToLowerInvariant(), StringComparer.Ordinal);
        }

        public static List<string> NormalizeItems(IEnumerable<string> rawItems, int maxItems = 30) {
            if (rawItems == null) {
                return new List<string>();
            }

            var units = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "tsp", "teaspoon", "teaspoons", "tbsp", "tablespoon", "tablespoons",
                "cup", "cups", "oz", "ounce", "ounces", "lb", "lbs", "pound", "pounds",
                "g", "gram", "grams", "kg", "ml", "l", "pinch", "dash", "clove", "cloves"
            };

            var normalized = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in rawItems) {
                if (string.IsNullOrWhiteSpace(raw)) {
                    continue;
                }

                var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var part in parts) {
                    var cleaned = Regex.Replace(part.ToLowerInvariant(), "[^a-z0-9\\s]", " ");
                    var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Where(t => !Regex.IsMatch(t, "^\\d+(?:\\/\\d+)?$"))
                        .Where(t => !Regex.IsMatch(t, "^\\d+(?:\\.\\d+)?$"))
                        .Where(t => !units.Contains(t))
                        .ToList();

                    if (tokens.Count == 0) {
                        continue;
                    }

                    var value = string.Join(" ", tokens);
                    if (seen.Add(value)) {
                        normalized.Add(value);
                    }

                    if (normalized.Count >= maxItems) {
                        return normalized;
                    }
                }
            }

            return normalized;
        }

        public static string BuildUrl(string service, List<string> normalizedItems, string zipCode = null, string store = null) {
            if (normalizedItems == null || normalizedItems.Count == 0) {
                return null;
            }

            var normalizedService = service.Trim().ToLowerInvariant();
            if (normalizedService == "manual") {
                return null;
            }

            var encodedItems = string.Join(",", normalizedItems.Select(Uri.EscapeDataString));
            var encodedSearch = Uri.EscapeDataString(string.Join(" ", normalizedItems));

            switch (normalizedService) {
                case "instacart":
                    return $"https://www.instacart.com/store/recipes?ingredients={encodedItems}";
                case "walmart":
                    return $"https://www.walmart.com/search?q={encodedSearch}";
                case "amazonfresh":
                    return $"https://www.amazon.com/s?k={encodedSearch}&i=amazonfresh";
                case "shipt":
                    return $"https://www.shipt.com/shop/search?query={encodedSearch}";
                default:
                    return null;
            }
        }
    }
}
