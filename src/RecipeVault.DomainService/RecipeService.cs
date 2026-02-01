using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Logging;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class RecipeService : IRecipeService {
        private readonly ILogger<RecipeService> logger;
        private readonly IRecipeRepository recipeRepository;

        public RecipeService(IRecipeRepository recipeRepository, ILogger<RecipeService> logger) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
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
            if (entity == null) {
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
    }
}
