using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.DomainService.Models;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class RecipeMixingFacade : IRecipeMixingFacade {
        private readonly IUnitOfWork uow;
        private readonly IRecipeService recipeService;
        private readonly IRecipeMixingService recipeMixingService;
        private readonly RecipeMapper recipeMapper;
        private readonly ILogger<RecipeMixingFacade> logger;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeMixingFacade(
            ILogger<RecipeMixingFacade> logger,
            IUnitOfWork uow,
            IRecipeService recipeService,
            IRecipeMixingService recipeMixingService,
            RecipeMapper recipeMapper,
            ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.recipeService = recipeService;
            this.recipeMixingService = recipeMixingService;
            this.recipeMapper = recipeMapper;
            this.logger = logger;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<MixedRecipePreviewDto> MixRecipesAsync(MixRecipesRequestDto request) {
            if (request == null) {
                throw new ArgumentException("Mix request is required", nameof(request));
            }

            if (request.RecipeAId == Guid.Empty || request.RecipeBId == Guid.Empty) {
                throw new ArgumentException("Both recipe IDs are required");
            }

            if (request.RecipeAId == request.RecipeBId) {
                throw new ArgumentException("Cannot mix a recipe with itself");
            }

            logger.LogInformation("Mixing recipes: {RecipeA} + {RecipeB}, mode={Mode}",
                request.RecipeAId, request.RecipeBId, request.Mode);

            var recipeA = await recipeService.GetRecipeAsync(request.RecipeAId);
            var recipeB = await recipeService.GetRecipeAsync(request.RecipeBId);

            var preview = await recipeMixingService.MixRecipesAsync(
                recipeA,
                recipeB,
                request.Intent,
                request.Mode);

            var previewDto = MapToDto(preview);
            // Track parent recipe IDs
            previewDto.RecipeAResourceId = recipeA.RecipeResourceId;
            previewDto.RecipeBResourceId = recipeB.RecipeResourceId;
            
            return previewDto;
        }

        public async Task<MixedRecipePreviewDto> RefineMixedRecipeAsync(RefineMixRequestDto request) {
            if (request?.Preview == null) {
                throw new ArgumentException("Preview is required", nameof(request));
            }

            logger.LogInformation("Refining mixed recipe: {Title}", request.Preview.Title);

            var preview = MapFromDto(request.Preview);
            var refinedPreview = await recipeMixingService.RefineMixedRecipeAsync(
                preview,
                request.RefinementNotes);

            var refinedDto = MapToDto(refinedPreview);
            // Preserve parent tracking metadata across refine calls so save can retain lineage.
            refinedDto.RecipeAResourceId = request.Preview.RecipeAResourceId;
            refinedDto.RecipeBResourceId = request.Preview.RecipeBResourceId;

            return refinedDto;
        }

        public async Task<RecipeDto> SaveMixedRecipeAsync(MixedRecipePreviewDto preview) {
            logger.LogInformation("Saving mixed recipe: {Title}", preview.Title);

            var previewModel = MapFromDto(preview);

            // Create a new recipe from the preview
            var recipe = new Recipe(
                title: previewModel.Title,
                yield: previewModel.Yield,
                prepTimeMinutes: previewModel.PrepTimeMinutes,
                cookTimeMinutes: previewModel.CookTimeMinutes,
                description: previewModel.Description,
                source: previewModel.Source,
                originalImageUrl: null,
                isPublic: false
            );

            // Set ingredients
            var ingredients = previewModel.Ingredients.Select((ing, index) => new RecipeIngredient(
                sortOrder: index + 1,
                quantity: ing.Quantity,
                unit: ing.Unit,
                item: ing.Item,
                preparation: ing.Preparation,
                rawText: ing.RawText
            )).ToList();
            recipe.SetIngredients(ingredients);

            // Set instructions
            var instructions = previewModel.Instructions.Select(inst => new RecipeInstruction(
                stepNumber: inst.StepNumber,
                instruction: inst.Instruction,
                rawText: inst.RawText
            )).ToList();
            recipe.SetInstructions(instructions);

            // Mix intent comes from the preview
            string mixIntent = previewModel.MixNotes;

            // Save the recipe (reusing existing recipe service pattern)
            var updateDto = new UpdateRecipeDto {
                Title = recipe.Title,
                Yield = recipe.Yield,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                Description = recipe.Description,
                Source = recipe.Source,
                OriginalImageUrl = recipe.OriginalImageUrl,
                Ingredients = recipe.Ingredients.Select(i => new UpdateRecipeIngredientDto {
                    SortOrder = i.SortOrder,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList(),
                Instructions = recipe.Instructions.Select(i => new UpdateRecipeInstructionDto {
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList()
            };

            var savedRecipe = await recipeService.CreateRecipeAsync(updateDto);
            
            // Set the mixing metadata if we have parent recipe IDs
            if (preview.RecipeAResourceId.HasValue && preview.RecipeBResourceId.HasValue) {
                var parentA = await recipeService.GetRecipeAsync(preview.RecipeAResourceId.Value);
                var parentB = await recipeService.GetRecipeAsync(preview.RecipeBResourceId.Value);
                savedRecipe.SetMixedFrom(parentA.RecipeId, parentB.RecipeId, mixIntent);
            }

            await uow.SaveChangesAsync();

            try {
                await recipeService.AnalyzeAndApplyDietaryTagsAsync(savedRecipe);
                await uow.SaveChangesAsync();
            } catch (Exception ex) {
                logger.LogWarning(ex, "Dietary tag analysis failed for mixed recipe");
            }

            return recipeMapper.MapToDto(savedRecipe, CurrentSubjectId);
        }

        private MixedRecipePreviewDto MapToDto(MixedRecipePreview preview) {
            return new MixedRecipePreviewDto {
                Title = preview.Title,
                Description = preview.Description,
                Yield = preview.Yield,
                PrepTimeMinutes = preview.PrepTimeMinutes,
                CookTimeMinutes = preview.CookTimeMinutes,
                Ingredients = preview.Ingredients?.Select(i => new MixedIngredientDto {
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText,
                    Attribution = i.Attribution
                }).ToList(),
                Instructions = preview.Instructions?.Select(i => new MixedInstructionDto {
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText,
                    Attribution = i.Attribution
                }).ToList(),
                MixNotes = preview.MixNotes,
                Source = preview.Source
            };
        }

        private MixedRecipePreview MapFromDto(MixedRecipePreviewDto dto) {
            return new MixedRecipePreview {
                Title = dto.Title,
                Description = dto.Description,
                Yield = dto.Yield,
                PrepTimeMinutes = dto.PrepTimeMinutes,
                CookTimeMinutes = dto.CookTimeMinutes,
                Ingredients = dto.Ingredients?.Select(i => new MixedIngredient {
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText,
                    Attribution = i.Attribution
                }).ToList(),
                Instructions = dto.Instructions?.Select(i => new MixedInstruction {
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText,
                    Attribution = i.Attribution
                }).ToList(),
                MixNotes = dto.MixNotes,
                Source = dto.Source
            };
        }
    }
}
