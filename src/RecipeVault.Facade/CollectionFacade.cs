using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CollectionFacade : ICollectionFacade {
        private readonly IUnitOfWork uow;
        private readonly ICollectionService collectionService;
        private readonly CollectionMapper mapper;
        private readonly ILogger<CollectionFacade> logger;
        private readonly IDistributedLockProvider lockProvider;
        private readonly ISubjectPrincipal subjectPrincipal;

        public CollectionFacade(ILogger<CollectionFacade> logger, IUnitOfWork uow, ICollectionService collectionService, CollectionMapper mapper, IDistributedLockProvider lockProvider, ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.collectionService = collectionService;
            this.mapper = mapper;
            this.logger = logger;
            this.lockProvider = lockProvider;
            this.subjectPrincipal = subjectPrincipal;
        }

        private static string GetLockName(Guid id) {
            return $"CollectionResourceId:{id}";
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<CollectionDto> CreateCollectionAsync(UpdateCollectionDto dto) {
            var collection = await collectionService.CreateCollectionAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(collection);
        }

        public async Task<CollectionDto> GetCollectionAsync(Guid resourceId) {
            await using (var tx = uow.BeginNoTracking()) {
                var collection = await collectionService.GetCollectionAsync(resourceId).ConfigureAwait(false);
                return mapper.MapToDto(collection);
            }
        }

        public async Task<PagedList<CollectionDto>> SearchCollectionsAsync(CollectionSearchDto search) {
            var collectionSearch = mapper.Map(search);
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var collections = await collectionService.SearchCollectionsAsync(collectionSearch).ConfigureAwait(false);
                return collections.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<CollectionDto> UpdateCollectionAsync(Guid resourceId, UpdateCollectionDto dto) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var collection = await collectionService.UpdateCollectionAsync(resourceId, dto).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(collection);
            }
        }

        public async Task DeleteCollectionAsync(Guid resourceId) {
            var lockName = GetLockName(resourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                await collectionService.DeleteCollectionAsync(resourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<CollectionDto> AddRecipeToCollectionAsync(Guid collectionResourceId, AddRecipeToCollectionDto dto) {
            var lockName = GetLockName(collectionResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var collection = await collectionService.AddRecipeToCollectionAsync(collectionResourceId, dto.RecipeResourceId, dto.SortOrder).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(collection);
            }
        }

        public async Task<CollectionDto> RemoveRecipeFromCollectionAsync(Guid collectionResourceId, Guid recipeResourceId) {
            var lockName = GetLockName(collectionResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var collection = await collectionService.RemoveRecipeFromCollectionAsync(collectionResourceId, recipeResourceId).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(collection);
            }
        }

        public async Task<CollectionDto> ReorderCollectionRecipesAsync(Guid collectionResourceId, ReorderCollectionRecipesDto dto) {
            var lockName = GetLockName(collectionResourceId);

            logger.LogDebug("Acquiring lock for {LockName}", lockName);
            await using (await lockProvider.AcquireLockAsync(lockName).ConfigureAwait(false)) {
                logger.LogDebug("Acquired lock for {LockName}", lockName);

                var recipeResourceIdToSortOrder = dto.Recipes.ToDictionary(r => r.RecipeResourceId, r => r.SortOrder);
                var collection = await collectionService.ReorderCollectionRecipesAsync(collectionResourceId, recipeResourceIdToSortOrder).ConfigureAwait(false);
                await uow.SaveChangesAsync().ConfigureAwait(false);
                return mapper.MapToDto(collection);
            }
        }

        public async Task ReorderCollectionsAsync(ReorderCollectionsDto dto) {
            var collectionOrders = dto.Collections.Select(c => (c.CollectionResourceId, c.SortOrder)).ToList();
            await collectionService.ReorderCollectionsAsync(collectionOrders).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<CollectionDto>> GetUserCollectionsAsync() {
            await using (var tx = uow.BeginNoTracking()) {
                var collections = await collectionService.GetUserCollectionsAsync().ConfigureAwait(false);
                return collections.Select(c => mapper.MapToDto(c)).ToList();
            }
        }

        public async Task<PagedList<CollectionDto>> GetPublicCollectionsAsync(string searchTerm, int pageNumber, int pageSize) {
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var collections = await collectionService.GetPublicCollectionsAsync(searchTerm, pageNumber, pageSize).ConfigureAwait(false);
                return collections.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<PagedList<CollectionDto>> GetFeaturedCollectionsAsync(int pageNumber, int pageSize) {
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var collections = await collectionService.GetFeaturedCollectionsAsync(pageNumber, pageSize).ConfigureAwait(false);
                return collections.Convert(x => mapper.MapToDto(x));
            }
        }
    }
}
