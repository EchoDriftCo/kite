using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
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

        public RecipeFacade(ILogger<RecipeFacade> logger, IUnitOfWork uow, IRecipeService recipeService, RecipeMapper mapper, IDistributedLockProvider lockProvider) {
            this.uow = uow;
            this.recipeService = recipeService;
            this.mapper = mapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
        }

        private static string GetLockName(Guid id) {
            return $"RecipeResourceId:{id}";
        }

        public async Task<RecipeDto> CreateRecipeAsync(UpdateRecipeDto dto) {
            var recipe = await recipeService.CreateRecipeAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(recipe);
        }

        public async Task<RecipeDto> GetRecipeAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipe = await recipeService.GetRecipeAsync(resourceId);
                return mapper.MapToDto(recipe);
            }
        }

        public async Task<PagedList<RecipeDto>> SearchRecipesAsync(RecipeSearchDto search) {
            var recipeSearch = mapper.Map(search);
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var recipes = await recipeService.SearchRecipesAsync(recipeSearch).ConfigureAwait(false);
                return recipes.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<RecipeDto> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var recipe = await recipeService.UpdateRecipeAsync(resourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(recipe);
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
    }
}
