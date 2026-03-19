using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    /// <summary>
    /// Facade for AI-powered recipe generation
    /// </summary>
    public class RecipeGenerationFacade : IRecipeGenerationFacade {
        private readonly IUnitOfWork uow;
        private readonly IRecipeGenerationService generationService;
        private readonly IRecipeService recipeService;
        private readonly ITagService tagService;
        private readonly RecipeMapper recipeMapper;
        private readonly ILogger<RecipeGenerationFacade> logger;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeGenerationFacade(
            ILogger<RecipeGenerationFacade> logger,
            IUnitOfWork uow,
            IRecipeGenerationService generationService,
            IRecipeService recipeService,
            ITagService tagService,
            RecipeMapper recipeMapper,
            ISubjectPrincipal subjectPrincipal) {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
            this.recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
            this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            this.recipeMapper = recipeMapper ?? throw new ArgumentNullException(nameof(recipeMapper));
            this.subjectPrincipal = subjectPrincipal ?? throw new ArgumentNullException(nameof(subjectPrincipal));
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        /// <summary>
        /// Generate new recipes based on user prompt and constraints
        /// </summary>
        public async Task<List<GeneratedRecipeDto>> GenerateRecipesAsync(GenerateRecipeRequestDto request) {
            logger.LogInformation("Generating {Count} recipe(s) for prompt: {Prompt}", request.Variations, request.Prompt);

            var recipes = await generationService.GenerateRecipesAsync(request).ConfigureAwait(false);

            logger.LogInformation("Generated {Count} recipe(s)", recipes.Count);
            return recipes;
        }

        /// <summary>
        /// Refine a previously generated recipe
        /// </summary>
        public async Task<GeneratedRecipeDto> RefineRecipeAsync(RefineRecipeRequestDto request) {
            logger.LogInformation("Refining recipe '{Title}'", request.PreviousRecipe?.Title);

            var refinedRecipe = await generationService.RefineRecipeAsync(request).ConfigureAwait(false);

            logger.LogInformation("Refined recipe to '{Title}'", refinedRecipe.Title);
            return refinedRecipe;
        }

        /// <summary>
        /// Save a generated recipe to the user's library
        /// </summary>
        public async Task<RecipeDto> SaveGeneratedRecipeAsync(GeneratedRecipeDto generatedRecipe) {
            logger.LogInformation("Saving generated recipe '{Title}' to library", generatedRecipe.Title);

            // Convert GeneratedRecipeDto to UpdateRecipeDto
            var updateDto = new UpdateRecipeDto {
                Title = generatedRecipe.Title,
                Description = generatedRecipe.Description,
                Yield = generatedRecipe.Yield,
                PrepTimeMinutes = generatedRecipe.PrepTimeMinutes,
                CookTimeMinutes = generatedRecipe.CookTimeMinutes,
                Source = "AI Generated",
                OriginalImageUrl = null,
                Ingredients = generatedRecipe.Ingredients?.Select((ing, index) => new UpdateRecipeIngredientDto {
                    SortOrder = index,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit,
                    Item = ing.Item,
                    Preparation = ing.Preparation,
                    RawText = FormatIngredientText(ing)
                }).ToList() ?? new List<UpdateRecipeIngredientDto>(),
                Instructions = generatedRecipe.Instructions?.Select(inst => new UpdateRecipeInstructionDto {
                    StepNumber = inst.StepNumber,
                    Instruction = inst.Instruction,
                    RawText = inst.Instruction
                }).ToList() ?? new List<UpdateRecipeInstructionDto>()
            };

            // Create the recipe
            var recipe = await recipeService.CreateRecipeAsync(updateDto).ConfigureAwait(false);

            // Add "AI Generated" tag
            await AddAiGeneratedTagAsync(recipe).ConfigureAwait(false);

            // Add suggested tags from the AI
            if (generatedRecipe.Tags != null && generatedRecipe.Tags.Count > 0) {
                await AddSuggestedTagsAsync(recipe, generatedRecipe.Tags).ConfigureAwait(false);
            }

            await uow.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Saved generated recipe {RecipeResourceId}", recipe.RecipeResourceId);

            return recipeMapper.MapToDto(recipe, CurrentSubjectId);
        }

        /// <summary>
        /// Get remaining generation quota for today
        /// </summary>
        public async Task<int> GetRemainingGenerationsAsync() {
            return await generationService.GetRemainingGenerationsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Add "AI Generated" tag to recipe
        /// </summary>
        private async Task AddAiGeneratedTagAsync(Recipe recipe) {
            try {
                const string aiGeneratedTagName = "AI Generated";
                var aiTag = await tagService.GetOrCreateTagAsync(aiGeneratedTagName, TagCategory.Source).ConfigureAwait(false);

                if (aiTag != null) {
                    var assignDto = new AssignTagDto {
                        TagResourceId = aiTag.TagResourceId
                    };

                    await recipeService.AssignTagsToRecipeAsync(recipe.RecipeResourceId, new List<AssignTagDto> { assignDto })
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "Failed to add AI Generated tag to recipe {RecipeResourceId}", recipe.RecipeResourceId);
            }
        }

        /// <summary>
        /// Add suggested tags from AI generation
        /// </summary>
        private async Task AddSuggestedTagsAsync(Recipe recipe, List<string> suggestedTags) {
            try {
                var tagDtos = new List<AssignTagDto>();

                foreach (var tagName in suggestedTags.Take(10)) // Limit to 10 tags
                {
                    try {
                        // Determine tag category based on tag name patterns
                        var category = DetermineTagCategory(tagName);
                        var tag = await tagService.GetOrCreateTagAsync(tagName, category).ConfigureAwait(false);
                        if (tag != null) {
                            tagDtos.Add(new AssignTagDto {
                                TagResourceId = tag.TagResourceId
                            });
                        }
                    }
                    catch (Exception ex) {
                        logger.LogDebug(ex, "Failed to create/add tag '{TagName}'", tagName);
                    }
                }

                if (tagDtos.Count > 0) {
                    await recipeService.AssignTagsToRecipeAsync(recipe.RecipeResourceId, tagDtos)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "Failed to add suggested tags to recipe {RecipeResourceId}", recipe.RecipeResourceId);
            }
        }

        /// <summary>
        /// Determine the appropriate tag category for a tag name
        /// </summary>
        private TagCategory DetermineTagCategory(string tagName) {
            var lowerTag = tagName.ToLowerInvariant();

            // Dietary tags
            if (lowerTag.Contains("vegan") || lowerTag.Contains("vegetarian") || lowerTag.Contains("gluten") ||
                lowerTag.Contains("dairy") || lowerTag.Contains("keto") || lowerTag.Contains("paleo")) {
                return TagCategory.Dietary;
            }

            // Cuisine tags
            if (lowerTag.Contains("italian") || lowerTag.Contains("mexican") || lowerTag.Contains("asian") ||
                lowerTag.Contains("french") || lowerTag.Contains("chinese") || lowerTag.Contains("japanese") ||
                lowerTag.Contains("thai") || lowerTag.Contains("indian") || lowerTag.Contains("american")) {
                return TagCategory.Cuisine;
            }

            // Meal type tags
            if (lowerTag.Contains("breakfast") || lowerTag.Contains("lunch") || lowerTag.Contains("dinner") ||
                lowerTag.Contains("dessert") || lowerTag.Contains("snack") || lowerTag.Contains("appetizer") ||
                lowerTag.Contains("side")) {
                return TagCategory.MealType;
            }

            // Default to Custom
            return TagCategory.Custom;
        }

        /// <summary>
        /// Format ingredient text for raw text display
        /// </summary>
        private string FormatIngredientText(GeneratedIngredientDto ingredient) {
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
