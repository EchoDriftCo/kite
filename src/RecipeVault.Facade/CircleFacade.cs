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
    public class CircleFacade : ICircleFacade {
        private readonly IUnitOfWork uow;
        private readonly ICircleService circleService;
        private readonly CircleMapper mapper;
        private readonly RecipeMapper recipeMapper;
        private readonly ILogger<CircleFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public CircleFacade(ILogger<CircleFacade> logger, IUnitOfWork uow, ICircleService circleService, CircleMapper mapper, RecipeMapper recipeMapper, IDistributedLockProvider lockProvider, ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.circleService = circleService;
            this.mapper = mapper;
            this.recipeMapper = recipeMapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
            this.subjectPrincipal = subjectPrincipal;
        }

        private static string GetLockName(Guid id) {
            return $"CircleResourceId:{id}";
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<CircleDto> CreateCircleAsync(UpdateCircleDto dto) {
            var circle = await circleService.CreateCircleAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(circle);
        }

        public async Task<CircleDto> GetCircleAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var circle = await circleService.GetCircleAsync(resourceId);
                return mapper.MapToDto(circle);
            }
        }

        public async Task<PagedList<CircleDto>> SearchCirclesAsync(CircleSearchDto search) {
            var circleSearch = mapper.Map(search);
            circleSearch.SubjectId = CurrentSubjectId;
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var circles = await circleService.SearchCirclesAsync(circleSearch).ConfigureAwait(false);
                return circles.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<CircleDto> UpdateCircleAsync(Guid resourceId, UpdateCircleDto dto) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var circle = await circleService.UpdateCircleAsync(resourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(circle);
            }
        }

        public async Task DeleteCircleAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await circleService.DeleteCircleAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<CircleInviteDto> InviteToCircleAsync(Guid circleResourceId, InviteToCircleDto dto) {
            var lockName = GetLockName(circleResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var invite = await circleService.InviteToCircleAsync(circleResourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(invite);
            }
        }

        public async Task<CircleDto> AcceptInviteAsync(Guid inviteToken) {
            var circle = await circleService.AcceptInviteAsync(inviteToken).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(circle);
        }

        public async Task<CircleInviteDto> GetInviteDetailsAsync(Guid inviteToken) {
            await using (var tx = uow.BeginNoTracking()) {
                var invite = await circleService.GetInviteDetailsAsync(inviteToken);
                return mapper.MapToDto(invite);
            }
        }

        public async Task RemoveMemberAsync(Guid circleResourceId, int subjectId) {
            var lockName = GetLockName(circleResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await circleService.RemoveMemberAsync(circleResourceId, subjectId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task LeaveCircleAsync(Guid circleResourceId) {
            var lockName = GetLockName(circleResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await circleService.LeaveCircleAsync(circleResourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<CircleDto> ShareRecipeToCircleAsync(Guid circleResourceId, ShareRecipeToCircleDto dto) {
            var lockName = GetLockName(circleResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var circle = await circleService.ShareRecipeToCircleAsync(circleResourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(circle);
            }
        }

        public async Task UnshareRecipeFromCircleAsync(Guid circleResourceId, Guid recipeResourceId) {
            var lockName = GetLockName(circleResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await circleService.UnshareRecipeFromCircleAsync(circleResourceId, recipeResourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<PagedList<RecipeDto>> GetCircleRecipesAsync(Guid circleResourceId, int pageNumber = 1, int pageSize = 20) {
            await using (var tx = uow.BeginNoTracking()) {
                var recipes = await circleService.GetCircleRecipesAsync(circleResourceId, pageNumber, pageSize);
                return recipes.Convert(x => recipeMapper.MapToDto(x));
            }
        }
    }
}
