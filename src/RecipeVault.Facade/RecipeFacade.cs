using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class RecipeFacade : IRecipeFacade {
        private readonly IUnitOfWork uow;
        private readonly IRecipeService recipeService;
        private readonly RecipeMapper mapper;
        private readonly ILogger<RecipeFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public RecipeFacade(ILogger<RecipeFacade> logger, IUnitOfWork uow, IRecipeService recipeService, RecipeMapper mapper, IDistributedLockProvider lockProvider, ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.recipeService = recipeService;
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
    }
}
