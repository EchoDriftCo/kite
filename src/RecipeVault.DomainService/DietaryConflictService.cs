using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public class DietaryConflictService : IDietaryConflictService {
        private readonly ILogger<DietaryConflictService> logger;
        private readonly IRecipeRepository recipeRepository;
        private readonly IDietaryProfileRepository profileRepository;

        // Map restriction codes to ingredient patterns
        private static readonly Dictionary<string, string[]> RestrictionIngredients = new() {
            // Allergies
            ["peanuts"] = new[] { "peanut", "peanuts", "peanut butter", "peanut oil" },
            ["tree-nuts"] = new[] { "almond", "cashew", "walnut", "pecan", "pistachio", "hazelnut", "macadamia", "pine nut" },
            ["shellfish"] = new[] { "shrimp", "crab", "lobster", "clam", "mussel", "oyster", "scallop", "crawfish" },
            ["fish"] = new[] { "salmon", "tuna", "cod", "tilapia", "halibut", "trout", "anchovies", "fish sauce" },
            ["eggs"] = new[] { "egg", "eggs", "egg white", "egg yolk", "mayonnaise" },
            ["dairy"] = new[] { "milk", "cream", "butter", "cheese", "yogurt", "whey", "casein", "lactose" },
            ["wheat"] = new[] { "wheat", "flour", "bread", "pasta", "couscous", "semolina", "wheat germ" },
            ["gluten"] = new[] { "wheat", "barley", "rye", "flour", "bread", "pasta", "couscous", "malt" },
            ["soy"] = new[] { "soy", "soybean", "tofu", "tempeh", "soy sauce", "edamame", "miso" },
            ["sesame"] = new[] { "sesame", "tahini", "sesame oil", "sesame seed" },
            
            // Intolerances
            ["lactose"] = new[] { "milk", "cream", "ice cream", "lactose" },
            
            // Dietary choices
            ["vegetarian"] = new[] { "meat", "beef", "pork", "chicken", "turkey", "lamb", "veal", "fish", "seafood", "shrimp", "salmon", "tuna" },
            ["vegan"] = new[] { "meat", "beef", "pork", "chicken", "turkey", "lamb", "veal", "fish", "seafood", "shrimp", "salmon", "tuna", "milk", "cream", "butter", "cheese", "yogurt", "egg", "eggs", "honey" },
            ["pescatarian"] = new[] { "meat", "beef", "pork", "chicken", "turkey", "lamb", "veal", "poultry" },
            ["keto"] = new[] { "sugar", "rice", "pasta", "bread", "potato", "flour", "corn" },
            ["paleo"] = new[] { "dairy", "grains", "legumes", "beans", "lentils", "peanuts", "sugar", "bread", "pasta", "rice" },
            ["halal"] = new[] { "pork", "bacon", "ham", "lard", "alcohol", "wine", "beer", "vodka" },
            ["kosher"] = new[] { "pork", "bacon", "ham", "lard", "shellfish", "shrimp", "crab", "lobster" }
        };

        public DietaryConflictService(
            ILogger<DietaryConflictService> logger,
            IRecipeRepository recipeRepository,
            IDietaryProfileRepository profileRepository) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
            this.profileRepository = profileRepository;
        }

        public async Task<DietaryConflictCheckDto> CheckRecipeByIdAsync(int recipeId, int profileId) {
            var recipe = await recipeRepository.GetByIdAsync(recipeId).ConfigureAwait(false);
            if (recipe == null) {
                throw new ArgumentException($"Recipe with ID {recipeId} not found");
            }

            var profile = await profileRepository.GetByIdAsync(profileId).ConfigureAwait(false);
            if (profile == null) {
                throw new ArgumentException($"Dietary profile with ID {profileId} not found");
            }

            return await CheckRecipeAsync(recipe, profile).ConfigureAwait(false);
        }

        public Task<DietaryConflictCheckDto> CheckRecipeAsync(Recipe recipe, DietaryProfile profile) {
            var conflicts = new List<DietaryConflictDto>();

            // Check restrictions
            foreach (var restriction in profile.Restrictions) {
                if (RestrictionIngredients.TryGetValue(restriction.RestrictionCode.ToLower(CultureInfo.InvariantCulture), out var patterns)) {
                    foreach (var ingredient in recipe.Ingredients) {
                        if (IngredientContainsPattern(ingredient, patterns)) {
                            conflicts.Add(new DietaryConflictDto {
                                IngredientIndex = ingredient.SortOrder,
                                IngredientText = ingredient.RawText,
                                RestrictionCode = restriction.RestrictionCode,
                                RestrictionType = restriction.RestrictionType,
                                Severity = restriction.Severity,
                                Message = $"Contains {restriction.RestrictionCode} ({restriction.RestrictionType})"
                            });
                        }
                    }
                } else {
                    logger.LogWarning("Unknown restriction code: {RestrictionCode}", restriction.RestrictionCode);
                }
            }

            // Check avoided ingredients
            foreach (var avoidedIngredient in profile.AvoidedIngredients) {
                foreach (var ingredient in recipe.Ingredients) {
                    if (IngredientContainsText(ingredient, avoidedIngredient.IngredientName)) {
                        conflicts.Add(new DietaryConflictDto {
                            IngredientIndex = ingredient.SortOrder,
                            IngredientText = ingredient.RawText,
                            RestrictionCode = avoidedIngredient.IngredientName,
                            RestrictionType = "AvoidedIngredient",
                            Severity = "Flexible",
                            Message = $"Contains avoided ingredient: {avoidedIngredient.IngredientName}" +
                                     (string.IsNullOrEmpty(avoidedIngredient.Reason) ? "" : $" ({avoidedIngredient.Reason})")
                        });
                    }
                }
            }

            // Remove duplicates (same ingredient flagged for multiple reasons)
            conflicts = conflicts
                .GroupBy(c => new { c.IngredientIndex, c.RestrictionCode })
                .Select(g => g.First())
                .ToList();

            var result = new DietaryConflictCheckDto {
                CanEat = conflicts.Count == 0,
                Conflicts = conflicts.OrderBy(c => c.IngredientIndex).ToList()
            };

            return Task.FromResult(result);
        }

        private static bool IngredientContainsPattern(RecipeIngredient ingredient, string[] patterns) {
            var ingredientText = $"{ingredient.Item} {ingredient.Preparation} {ingredient.Unit}".ToLower(CultureInfo.InvariantCulture);
            return patterns.Any(pattern => ingredientText.Contains(pattern.ToLower(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase));
        }

        private static bool IngredientContainsText(RecipeIngredient ingredient, string searchText) {
            var ingredientText = $"{ingredient.Item} {ingredient.Preparation}".ToLower(CultureInfo.InvariantCulture);
            return ingredientText.Contains(searchText.ToLower(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }
    }
}
