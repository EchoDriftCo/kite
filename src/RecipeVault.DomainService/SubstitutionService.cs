using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for handling ingredient substitution suggestions and application
    /// </summary>
    public class SubstitutionService : ISubstitutionService {
        private readonly IGeminiClient geminiClient;
        private readonly IRecipeService recipeService;
        private readonly ISubstitutionCacheService cacheService;
        private readonly ILogger<SubstitutionService> logger;

        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

        public SubstitutionService(
            IGeminiClient geminiClient,
            IRecipeService recipeService,
            ISubstitutionCacheService cacheService,
            ILogger<SubstitutionService> logger) {
            this.geminiClient = geminiClient ?? throw new ArgumentNullException(nameof(geminiClient));
            this.recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
            this.cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get substitution suggestions for a recipe
        /// </summary>
        public async Task<SubstitutionResponseDto> GetSubstitutionsAsync(
            Guid recipeResourceId,
            List<int> ingredientIndices,
            List<string> dietaryConstraints) {

            // Validate input
            var hasIndices = ingredientIndices != null && ingredientIndices.Count > 0;
            var hasConstraints = dietaryConstraints != null && dietaryConstraints.Count > 0;
            
            if (!hasIndices && !hasConstraints) {
                throw new ArgumentException("Must provide at least one ingredient index or dietary constraint");
            }

            // Get the recipe
            var recipe = await recipeService.GetRecipeAsync(recipeResourceId).ConfigureAwait(false);
            
            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0) {
                throw new InvalidOperationException("Recipe has no ingredients to substitute");
            }

            // Validate ingredient indices
            if (hasIndices) {
                var maxIndex = recipe.Ingredients.Count - 1;
                var invalidIndices = ingredientIndices.Where(i => i < 0 || i > maxIndex).ToList();
                if (invalidIndices.Count > 0) {
                    throw new ArgumentException($"Invalid ingredient indices: {string.Join(", ", invalidIndices)}. Recipe has {recipe.Ingredients.Count} ingredients (0-{maxIndex})");
                }
            }

            // Check cache
            var cacheKey = cacheService.BuildCacheKey(recipeResourceId, ingredientIndices, dietaryConstraints);
            var cached = await cacheService.GetAsync<SubstitutionResponseDto>(cacheKey).ConfigureAwait(false);
            
            if (cached != null) {
                logger.LogInformation("Returning cached substitution analysis for recipe {RecipeId}", recipeResourceId);
                cached.Cached = true;
                return cached;
            }

            logger.LogInformation("Analyzing substitutions for recipe {RecipeId}, ingredientCount={Count}, constraintCount={ConstraintCount}",
                recipeResourceId, ingredientIndices?.Count ?? 0, dietaryConstraints?.Count ?? 0);

            // Prepare data for Gemini
            var allIngredients = recipe.Ingredients
                .OrderBy(i => i.SortOrder)
                .Select(i => FormatIngredient(i))
                .ToList();

            var instructionsSummary = recipe.Instructions != null && recipe.Instructions.Any()
                ? string.Join(" ", recipe.Instructions.OrderBy(i => i.StepNumber).Select(i => i.Instruction).Take(5))
                : null;

            // Call Gemini
            var analysis = await geminiClient.AnalyzeSubstitutionsAsync(
                recipe.Title,
                allIngredients,
                instructionsSummary,
                ingredientIndices,
                dietaryConstraints
            ).ConfigureAwait(false);

            // Map to DTO
            var response = new SubstitutionResponseDto {
                Analysis = analysis.Analysis,
                Substitutions = analysis.Substitutions?.Select(s => new IngredientSubstitutionDto {
                    OriginalIndex = s.OriginalIndex,
                    OriginalText = s.Original,
                    Reason = s.Reason,
                    Options = s.Options?.Select(o => new SubstitutionOptionDto {
                        Name = o.Name,
                        Ingredients = o.Ingredients?.Select(i => new SubstitutionIngredientDto {
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            Item = i.Item
                        }).ToList() ?? new List<SubstitutionIngredientDto>(),
                        Notes = o.Notes,
                        TechniqueAdjustments = o.TechniqueAdjustments
                    }).ToList() ?? new List<SubstitutionOptionDto>()
                }).ToList() ?? new List<IngredientSubstitutionDto>(),
                Cached = false
            };

            // Cache the result
            await cacheService.SetAsync(cacheKey, response, CacheTtl).ConfigureAwait(false);

            logger.LogInformation("Successfully analyzed substitutions for recipe {RecipeId}, found {Count} ingredients with options",
                recipeResourceId, response.Substitutions.Count);

            return response;
        }

        /// <summary>
        /// Resolve substitution selections - if OptionIndex is provided, fetch the option data from cache
        /// </summary>
        private async Task ResolveSelectionsAsync(Guid recipeResourceId, List<SubstitutionSelectionDto> selections) {
            // Check if any selections need resolution (have OptionIndex but not SelectedOption)
            var selectionsNeedingResolution = selections
                .Where(s => s.OptionIndex.HasValue && s.SelectedOption == null)
                .ToList();

            if (selectionsNeedingResolution.Count == 0) {
                return; // All selections already have SelectedOption
            }

            logger.LogInformation("Resolving {Count} selections from OptionIndex for recipe {RecipeId}",
                selectionsNeedingResolution.Count, recipeResourceId);

            // We need to fetch substitutions to resolve the option indices
            // Build a cache key that would include all ingredient indices that need resolution
            var ingredientIndices = selectionsNeedingResolution
                .Select(s => s.IngredientIndex)
                .Distinct()
                .OrderBy(i => i)
                .ToList();

            // Try to find cached substitutions - first try exact match with these indices
            var cacheKey = cacheService.BuildCacheKey(recipeResourceId, ingredientIndices, null);
            var cached = await cacheService.GetAsync<SubstitutionResponseDto>(cacheKey).ConfigureAwait(false);

            // If not found, try with null indices (dietary constraint mode) as a fallback
            if (cached == null) {
                cacheKey = cacheService.BuildCacheKey(recipeResourceId, null, null);
                cached = await cacheService.GetAsync<SubstitutionResponseDto>(cacheKey).ConfigureAwait(false);
            }

            if (cached == null) {
                throw new InvalidOperationException(
                    "Cannot resolve substitution options by index - substitution data not found in cache. " +
                    "Please provide full SelectedOption data or ensure substitutions were recently fetched.");
            }

            // Resolve each selection
            foreach (var selection in selectionsNeedingResolution) {
                var substitution = cached.Substitutions.FirstOrDefault(s => s.OriginalIndex == selection.IngredientIndex);
                
                if (substitution == null) {
                    throw new ArgumentException(
                        $"No substitution found for ingredient index {selection.IngredientIndex}. " +
                        "Ensure you're selecting from the current substitution suggestions.");
                }

                if (!selection.OptionIndex.HasValue || selection.OptionIndex.Value < 0 || selection.OptionIndex.Value >= substitution.Options.Count) {
                    throw new ArgumentException(
                        $"Invalid OptionIndex {selection.OptionIndex} for ingredient index {selection.IngredientIndex}. " +
                        $"Valid range is 0-{substitution.Options.Count - 1}.");
                }

                // Set the SelectedOption from the cached data
                selection.SelectedOption = substitution.Options[selection.OptionIndex.Value];
            }

            logger.LogInformation("Successfully resolved {Count} selections from cache", selectionsNeedingResolution.Count);
        }

        /// <summary>
        /// Apply selected substitutions and create a forked recipe
        /// </summary>
        public async Task<Recipe> ApplySubstitutionsAsync(
            Guid recipeResourceId,
            List<SubstitutionSelectionDto> selections,
            string forkTitle = null) {

            if (selections == null || selections.Count == 0) {
                throw new ArgumentException("Must provide at least one substitution selection", nameof(selections));
            }

            // Get the original recipe (needed for both validation and forking)
            var original = await recipeService.GetRecipeAsync(recipeResourceId).ConfigureAwait(false);

            // Resolve all selections to have full SelectedOption data
            await ResolveSelectionsAsync(recipeResourceId, selections).ConfigureAwait(false);

            // Validate all selections now have the required data
            foreach (var selection in selections) {
                if (selection.SelectedOption == null) {
                    throw new ArgumentException(
                        $"Selection for ingredient index {selection.IngredientIndex} is missing substitution data. " +
                        $"OptionIndex={selection.OptionIndex}. The substitution cache may have expired - please try again.");
                }
                if (selection.SelectedOption.Ingredients == null || selection.SelectedOption.Ingredients.Count == 0) {
                    throw new ArgumentException(
                        $"Selection for ingredient index {selection.IngredientIndex} has no substitute ingredients defined. " +
                        $"Option='{selection.SelectedOption.Name}'. This may be a data issue - please select a different option.");
                }
            }

            logger.LogInformation("Applying {Count} substitutions to recipe {RecipeId}",
                selections.Count, recipeResourceId);

            // Fork the recipe first
            var fork = await recipeService.ForkRecipeAsync(recipeResourceId, forkTitle).ConfigureAwait(false);

            // Build new ingredient list with substitutions applied
            var originalIngredients = fork.Ingredients.OrderBy(i => i.SortOrder).ToList();
            var newIngredients = new List<RecipeIngredient>();
            var substitutionNotes = new List<string>();
            var sortOrder = 0;

            for (int i = 0; i < originalIngredients.Count; i++) {
                var originalIngredient = originalIngredients[i];
                var selection = selections.FirstOrDefault(s => s.IngredientIndex == i);

                if (selection == null) {
                    // No substitution for this ingredient, keep original
                    newIngredients.Add(new RecipeIngredient(
                        sortOrder++,
                        originalIngredient.Quantity,
                        originalIngredient.Unit,
                        originalIngredient.Item,
                        originalIngredient.Preparation,
                        originalIngredient.RawText
                    ));
                } else {
                    // Apply the substitution
                    var option = selection.SelectedOption;
                    var originalText = FormatIngredient(originalIngredient);
                    
                    // Add each replacement ingredient
                    foreach (var substIngredient in option.Ingredients) {
                        newIngredients.Add(new RecipeIngredient(
                            sortOrder++,
                            substIngredient.Quantity,
                            substIngredient.Unit,
                            substIngredient.Item,
                            null, // No preparation notes from substitution
                            null  // No raw text
                        ));
                    }
                    
                    // Build substitution note
                    var substitutionDetail = option.Ingredients.Count == 1
                        ? FormatSubstitutionIngredient(option.Ingredients[0])
                        : string.Join(" + ", option.Ingredients.Select(FormatSubstitutionIngredient));
                    
                    var note = $"• **{originalText}** → {substitutionDetail}";
                    if (!string.IsNullOrWhiteSpace(option.Notes)) {
                        note += $"\n  _{option.Notes}_";
                    }
                    if (!string.IsNullOrWhiteSpace(option.TechniqueAdjustments)) {
                        note += $"\n  ⚠️ {option.TechniqueAdjustments}";
                    }
                    
                    substitutionNotes.Add(note);
                }
            }

            // Update the fork with new ingredients
            fork.SetIngredients(newIngredients);

            // Add note about substitutions to the description
            if (substitutionNotes.Count > 0) {
                var substitutionNote = $"\n\n---\n**Substitutions Made:**\n{string.Join("\n\n", substitutionNotes)}";
                
                fork.Update(
                    fork.Title,
                    fork.Yield,
                    fork.PrepTimeMinutes,
                    fork.CookTimeMinutes,
                    (fork.Description ?? "") + substitutionNote,
                    fork.Source,
                    fork.OriginalImageUrl
                );
            }

            logger.LogInformation("Successfully applied {Count} substitutions to forked recipe {ForkId}",
                selections.Count, fork.RecipeResourceId);

            return fork;
        }

        /// <summary>
        /// Format a substitution ingredient for display
        /// </summary>
        private static string FormatSubstitutionIngredient(SubstitutionIngredientDto ingredient) {
            var parts = new List<string>();
            
            if (ingredient.Quantity.HasValue) {
                parts.Add(ingredient.Quantity.Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
            }
            
            if (!string.IsNullOrWhiteSpace(ingredient.Unit)) {
                parts.Add(ingredient.Unit);
            }
            
            parts.Add(ingredient.Item);
            
            return string.Join(" ", parts);
        }

        /// <summary>
        /// Format an ingredient for display
        /// </summary>
        private static string FormatIngredient(RecipeIngredient ingredient) {
            var parts = new List<string>();
            
            if (ingredient.Quantity.HasValue) {
                parts.Add(ingredient.Quantity.Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
            }
            
            if (!string.IsNullOrWhiteSpace(ingredient.Unit)) {
                parts.Add(ingredient.Unit);
            }
            
            parts.Add(ingredient.Item);
            
            if (!string.IsNullOrWhiteSpace(ingredient.Preparation)) {
                parts.Add($"({ingredient.Preparation})");
            }
            
            return string.Join(" ", parts);
        }
    }
}
