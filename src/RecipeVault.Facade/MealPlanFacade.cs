using System;
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
    public class MealPlanFacade : IMealPlanFacade {
        private readonly IUnitOfWork uow;
        private readonly IMealPlanService mealPlanService;
        private readonly MealPlanMapper mapper;
        private readonly ILogger<MealPlanFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public MealPlanFacade(ILogger<MealPlanFacade> logger, IUnitOfWork uow, IMealPlanService mealPlanService, MealPlanMapper mapper, IDistributedLockProvider lockProvider, ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.mealPlanService = mealPlanService;
            this.mapper = mapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
            this.subjectPrincipal = subjectPrincipal;
        }

        private static string GetLockName(Guid id) {
            return $"MealPlanResourceId:{id}";
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<MealPlanDto> CreateMealPlanAsync(UpdateMealPlanDto dto) {
            var mealPlan = await mealPlanService.CreateMealPlanAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(mealPlan);
        }

        public async Task<MealPlanDto> GetMealPlanAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var mealPlan = await mealPlanService.GetMealPlanAsync(resourceId);
                return mapper.MapToDto(mealPlan);
            }
        }

        public async Task<PagedList<MealPlanDto>> SearchMealPlansAsync(MealPlanSearchDto search) {
            var mealPlanSearch = mapper.Map(search);
            mealPlanSearch.CreatedSubjectId = CurrentSubjectId;
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var mealPlans = await mealPlanService.SearchMealPlansAsync(mealPlanSearch).ConfigureAwait(false);
                return mealPlans.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<MealPlanDto> UpdateMealPlanAsync(Guid resourceId, UpdateMealPlanDto dto) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var mealPlan = await mealPlanService.UpdateMealPlanAsync(resourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(mealPlan);
            }
        }

        public async Task DeleteMealPlanAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await mealPlanService.DeleteMealPlanAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<GroceryListDto> GetGroceryListAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                return await mealPlanService.GenerateGroceryListAsync(resourceId).ConfigureAwait(false);
            }
        }
    }
}
