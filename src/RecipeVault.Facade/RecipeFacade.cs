using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class RecipeFacade : IRecipeFacade {
        private readonly IUnitOfWork uow;
        private readonly IRecipeService recipeService;
        private readonly ISubstitutionService substitutionService;
        private readonly RecipeMapper mapper;
        private readonly ILogger<RecipeFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeFacade(ILogger<RecipeFacade> logger, IUnitOfWork uow, IRecipeService recipeService, ISubstitutionService substitutionService, RecipeMapper mapper, IDistributedLockProvider lockProvider, ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.recipeService = recipeService;
            this.substitutionService = substitutionService;
            this.mapper = mapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
            this.subjectPrincipal = subjectPrincipal;
        }

        private static string GetLockName(Guid id) {
            return $"RecipeResourceId:{id}";
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<RecipeDto> CreateRecipeAsync(UpdateRecipeDto dto) {
            var recipe = await recipeService.CreateRecipeAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);

            try {
                await recipeService.AnalyzeAndApplyDietaryTagsAsync(recipe).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            } catch (Exception ex) {
                logger.LogWarning(ex, "Dietary tag analysis failed for recipe {RecipeResourceId}", recipe.RecipeResourceId);
            }

            return mapper.MapToDto(recipe, CurrentSubjectId);
        }

        public async Task<RecipeDto> GetRecipeAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeService.GetRecipeAsync(resourceId);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<PagedList<RecipeDto>> SearchRecipesAsync(RecipeSearchDto search) {
            var recipeSearch = mapper.Map(search);
            recipeSearch.CreatedSubjectId = CurrentSubjectId;
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var recipes = await recipeService.SearchRecipesAsync(recipeSearch).ConfigureAwait(false);
                var currentSubjectId = CurrentSubjectId;

                return recipes.Convert(x => mapper.MapToDto(x, currentSubjectId));
            }
        }

        public async Task<PagedList<RecipeDto>> DiscoverRecipesAsync(RecipeSearchDto search) {
            var recipeSearch = mapper.Map(search);
            // Discover returns all public recipes — no owner filter
            recipeSearch.IsPublic = true;
            recipeSearch.SearchingUserId = CurrentSubjectId;
            // Default sort for discover is newest
            if (string.IsNullOrEmpty(recipeSearch.SortBy)) {
                recipeSearch.SortBy = "CreatedDate";
                recipeSearch.SortDirection = "desc";
            }
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var recipes = await recipeService.SearchRecipesAsync(recipeSearch).ConfigureAwait(false);
                var currentSubjectId = CurrentSubjectId;
                return recipes.Convert(x => mapper.MapToDto(x, currentSubjectId));
            }
        }

        public async Task<RecipeDto> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var recipe = await recipeService.UpdateRecipeAsync(resourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                try {
                    await recipeService.AnalyzeAndApplyDietaryTagsAsync(recipe).ConfigureAwait(false);
                    await uow.SaveChangesAsync().ConfigureAwait(false);
                } catch (Exception ex) {
                    logger.LogWarning(ex, "Dietary tag analysis failed for recipe {RecipeResourceId}", recipe.RecipeResourceId);
                }

                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> SetRecipeVisibilityAsync(Guid resourceId, bool isPublic) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await recipeService.SetRecipeVisibilityAsync(resourceId, isPublic).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                var recipe = await recipeService.GetRecipeAsync(resourceId).ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task DeleteRecipeAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await recipeService.DeleteRecipeAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<ParseRecipeResponseDto> ParseRecipeImageAsync(ParseRecipeRequestDto request) {
            // No transaction needed - read-only external API call
            var result = await recipeService.ParseRecipeImageAsync(request).ConfigureAwait(false);
            return result;
        }

        public async Task<RecipeDto> AssignTagsAsync(Guid recipeResourceId, List<AssignTagDto> tags) {
            var lockName = GetLockName(recipeResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var recipe = await recipeService.AssignTagsToRecipeAsync(recipeResourceId, tags).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> RemoveTagAsync(Guid recipeResourceId, Guid tagResourceId) {
            var lockName = GetLockName(recipeResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var recipe = await recipeService.RemoveTagFromRecipeAsync(recipeResourceId, tagResourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> SetRecipeRatingAsync(Guid resourceId, int? rating) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await recipeService.SetRecipeRatingAsync(resourceId, rating).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                var recipe = await recipeService.GetRecipeAsync(resourceId).ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> SetRecipeFavoriteAsync(Guid resourceId, bool isFavorite) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await recipeService.SetRecipeFavoriteAsync(resourceId, isFavorite).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                var recipe = await recipeService.GetRecipeAsync(resourceId).ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> GenerateShareTokenAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await recipeService.GenerateShareTokenAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                var recipe = await recipeService.GetRecipeAsync(resourceId).ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> RevokeShareTokenAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await recipeService.RevokeShareTokenAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                var recipe = await recipeService.GetRecipeAsync(resourceId).ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> GetRecipeByShareTokenAsync(string shareToken) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeService.GetRecipeByShareTokenAsync(shareToken).ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> AnalyzeDietaryTagsAsync(Guid recipeResourceId) {
            var lockName = GetLockName(recipeResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var recipe = await recipeService.GetRecipeAsync(recipeResourceId).ConfigureAwait(false);
                await recipeService.AnalyzeAndApplyDietaryTagsAsync(recipe).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(recipe, CurrentSubjectId);
            }
        }

        public async Task<RecipeDto> ForkRecipeAsync(Guid recipeResourceId, string newTitle = null) {
            var fork = await recipeService.ForkRecipeAsync(recipeResourceId, newTitle).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);

            return mapper.MapToDto(fork, CurrentSubjectId);
        }

        public async Task<PagedList<RecipeDto>> GetRecipeForksAsync(Guid recipeResourceId, int pageNumber = 1, int pageSize = 20) {
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var forks = await recipeService.GetRecipeForksAsync(recipeResourceId, pageNumber, pageSize).ConfigureAwait(false);
                var currentSubjectId = CurrentSubjectId;
                return forks.Convert(x => mapper.MapToDto(x, currentSubjectId));
            }
        }

        public async Task<SubstitutionResponseDto> GetSubstitutionsAsync(Guid recipeResourceId, SubstitutionRequestDto request) {
            // No transaction needed - this is a read operation with external API call
            var result = await substitutionService.GetSubstitutionsAsync(
                recipeResourceId,
                request.IngredientIndices,
                request.DietaryConstraints
            ).ConfigureAwait(false);

            return result;
        }

        public async Task<RecipeDto> ApplySubstitutionsAsync(Guid recipeResourceId, ApplySubstitutionsDto request) {
            var fork = await substitutionService.ApplySubstitutionsAsync(
                recipeResourceId,
                request.Selections,
                request.ForkTitle
            ).ConfigureAwait(false);

            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(fork, CurrentSubjectId);
        }

        public Task<CookingDataDto> GetCookingDataAsync(Guid recipeResourceId) {
            return recipeService.GetCookingDataAsync(recipeResourceId);
        }
    }
}
