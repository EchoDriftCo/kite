using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    public class RecipeService : IRecipeService {
        private readonly ILogger<RecipeService> logger;
        private readonly IRecipeRepository recipeRepository;
        private readonly IGeminiClient geminiClient;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeService(IRecipeRepository recipeRepository, IGeminiClient geminiClient, ILogger<RecipeService> logger, ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
            this.geminiClient = geminiClient;
            this.subjectPrincipal = subjectPrincipal;
        }

        public async Task<Recipe> CreateRecipeAsync(UpdateRecipeDto dto) {
            var entity = new Recipe(dto.Title, dto.Yield, dto.PrepTimeMinutes, dto.CookTimeMinutes, dto.Description, dto.Source, dto.OriginalImageUrl);

            if (dto.Ingredients != null) {
                entity.SetIngredients(dto.Ingredients.Select(i => new RecipeIngredient(i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText)).ToList());
            }

            if (dto.Instructions != null) {
                entity.SetInstructions(dto.Instructions.Select(i => new RecipeInstruction(i.StepNumber, i.Instruction, i.RawText)).ToList());
            }

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                await recipeRepository.AddAsync(entity);
                logger.LogInformation("Created new recipe");
                return entity;
            }
        }

        public async Task<Recipe> GetRecipeAsync(Guid recipeResourceId) {
            var entity = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (entity == null || entity.CreatedSubject?.SubjectId != Guid.Parse(subjectPrincipal.SubjectId)) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            return entity;
        }

        public Task<PagedList<Recipe>> SearchRecipesAsync(RecipeSearch search) {
            return recipeRepository.SearchAsync(search);
        }

        public async Task<Recipe> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto) {
            var entity = await GetRecipeAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.Update(dto.Title, dto.Yield, dto.PrepTimeMinutes, dto.CookTimeMinutes, dto.Description, dto.Source, dto.OriginalImageUrl);

                if (dto.Ingredients != null) {
                    entity.SetIngredients(dto.Ingredients.Select(i => new RecipeIngredient(i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText)).ToList());
                }

                if (dto.Instructions != null) {
                    entity.SetInstructions(dto.Instructions.Select(i => new RecipeInstruction(i.StepNumber, i.Instruction, i.RawText)).ToList());
                }

                logger.LogInformation("Updated existing recipe");
                return entity;
            }
        }

        public async Task DeleteRecipeAsync(Guid resourceId) {
            var entity = await GetRecipeAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                await recipeRepository.RemoveAsync(entity);
                logger.LogInformation("Deleted recipe");
            }
        }

        public async Task<ParseRecipeResponseDto> ParseRecipeImageAsync(ParseRecipeRequestDto request) {
            logger.LogInformation("Parsing recipe image, mimeType={MimeType}, imageSize={ImageSize}",
                request.MimeType, request.Image?.Length ?? 0);

            try {
                var geminiResponse = await geminiClient.ParseRecipeAsync(request.Image, request.MimeType)
                    .ConfigureAwait(false);

                var result = new ParseRecipeResponseDto {
                    Confidence = geminiResponse.Confidence,
                    Parsed = new ParsedRecipeDto {
                        Title = geminiResponse.Title,
                        Yield = geminiResponse.Yield,
                        PrepTimeMinutes = geminiResponse.PrepTimeMinutes,
                        CookTimeMinutes = geminiResponse.CookTimeMinutes,
                        Ingredients = geminiResponse.Ingredients?.Select(i => new ParsedIngredientDto {
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            Item = i.Item,
                            Preparation = i.Preparation,
                            RawText = i.RawText
                        }).ToList(),
                        Instructions = geminiResponse.Instructions?.Select(i => new ParsedInstructionDto {
                            StepNumber = i.StepNumber,
                            Instruction = i.Instruction,
                            RawText = i.RawText
                        }).ToList()
                    },
                    Warnings = geminiResponse.Warnings
                };

                logger.LogInformation("Successfully parsed recipe, confidence={Confidence}, warnings={WarningCount}",
                    result.Confidence, result.Warnings?.Count ?? 0);

                return result;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Failed to parse recipe image");
                throw;
            }
        }
    }
}
