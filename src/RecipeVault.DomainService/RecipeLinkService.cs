using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class RecipeLinkService : IRecipeLinkService {
        private readonly ILogger<RecipeLinkService> logger;
        private readonly IRecipeLinkRepository recipeLinkRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeLinkService(
            ILogger<RecipeLinkService> logger,
            IRecipeLinkRepository recipeLinkRepository,
            IRecipeRepository recipeRepository,
            ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.recipeLinkRepository = recipeLinkRepository;
            this.recipeRepository = recipeRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        private async Task<Recipe> GetOwnRecipeAsync(Guid recipeResourceId) {
            var entity = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (entity == null || entity.CreatedSubject?.SubjectId != Guid.Parse(subjectPrincipal.SubjectId)) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }
            return entity;
        }

        public async Task<RecipeLink> CreateRecipeLinkAsync(Guid parentRecipeResourceId, CreateRecipeLinkDto dto) {
            var parentRecipe = await GetOwnRecipeAsync(parentRecipeResourceId).ConfigureAwait(false);
            var linkedRecipe = await recipeRepository.GetAsync(dto.LinkedRecipeResourceId).ConfigureAwait(false);

            if (linkedRecipe == null) {
                throw new RecipeNotFoundException($"Linked recipe with id {dto.LinkedRecipeResourceId} not found");
            }

            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
            if (linkedRecipe.CreatedSubject?.SubjectId != currentSubjectId) {
                throw new RecipeNotFoundException($"Linked recipe with id {dto.LinkedRecipeResourceId} not found");
            }

            // Prevent self-linking
            if (parentRecipe.RecipeId == linkedRecipe.RecipeId) {
                throw new InvalidOperationException("A recipe cannot link to itself");
            }

            // Check for circular links
            var hasCircularLink = await recipeLinkRepository.HasCircularLinkAsync(parentRecipe.RecipeId, linkedRecipe.RecipeId).ConfigureAwait(false);
            if (hasCircularLink) {
                throw new InvalidOperationException("Creating this link would create a circular dependency");
            }

            var recipeLink = new RecipeLink(
                parentRecipe.RecipeId,
                linkedRecipe.RecipeId,
                dto.IngredientIndex,
                dto.DisplayText,
                dto.IncludeInTotalTime,
                dto.PortionUsed
            );

            using (logger.PushProperty("RecipeLinkResourceId", recipeLink.RecipeLinkResourceId)) {
                await recipeLinkRepository.AddAsync(recipeLink);
                logger.LogInformation("Created recipe link from {ParentRecipeId} to {LinkedRecipeId}", parentRecipe.RecipeId, linkedRecipe.RecipeId);
                return recipeLink;
            }
        }

        public async Task<RecipeLink> UpdateRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId, UpdateRecipeLinkDto dto) {
            var parentRecipe = await GetOwnRecipeAsync(parentRecipeResourceId).ConfigureAwait(false);
            var recipeLink = await recipeLinkRepository.GetAsync(linkResourceId).ConfigureAwait(false);

            if (recipeLink == null || recipeLink.ParentRecipeId != parentRecipe.RecipeId) {
                throw new RecipeNotFoundException($"Recipe link with id {linkResourceId} not found");
            }

            using (logger.PushProperty("RecipeLinkResourceId", recipeLink.RecipeLinkResourceId)) {
                recipeLink.Update(dto.IngredientIndex, dto.DisplayText, dto.IncludeInTotalTime, dto.PortionUsed);
                logger.LogInformation("Updated recipe link");
                return recipeLink;
            }
        }

        public async Task DeleteRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId) {
            var parentRecipe = await GetOwnRecipeAsync(parentRecipeResourceId).ConfigureAwait(false);
            var recipeLink = await recipeLinkRepository.GetAsync(linkResourceId).ConfigureAwait(false);

            if (recipeLink == null || recipeLink.ParentRecipeId != parentRecipe.RecipeId) {
                throw new RecipeNotFoundException($"Recipe link with id {linkResourceId} not found");
            }

            using (logger.PushProperty("RecipeLinkResourceId", recipeLink.RecipeLinkResourceId)) {
                await recipeLinkRepository.RemoveAsync(recipeLink);
                logger.LogInformation("Deleted recipe link");
            }
        }

        public async Task<List<RecipeLink>> GetLinkedRecipesAsync(Guid parentRecipeResourceId) {
            var parentRecipe = await GetOwnRecipeAsync(parentRecipeResourceId).ConfigureAwait(false);
            return await recipeLinkRepository.GetLinkedRecipesAsync(parentRecipe.RecipeId).ConfigureAwait(false);
        }

        public async Task<List<RecipeLink>> GetUsedInRecipesAsync(Guid recipeResourceId) {
            var recipe = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            return await recipeLinkRepository.GetUsedInRecipesAsync(recipe.RecipeId).ConfigureAwait(false);
        }

        public async Task<List<Recipe>> SearchLinkableRecipesAsync(string query) {
            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
            
            // Search all user's recipes that match the query
            var allRecipes = await recipeRepository.SearchAsync(new Data.Searches.RecipeSearch {
                CreatedSubjectId = currentSubjectId,
                Title = query,
                PageSize = 50,
                PageNumber = 1
            }).ConfigureAwait(false);

            return allRecipes.Items.ToList();
        }
    }
}
