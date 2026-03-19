using System;
using System.Collections.Generic;
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
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class CollectionService : ICollectionService {
        private readonly ILogger<CollectionService> logger;
        private readonly ICollectionRepository collectionRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public CollectionService(ICollectionRepository collectionRepository, IRecipeRepository recipeRepository, ILogger<CollectionService> logger, ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.collectionRepository = collectionRepository;
            this.recipeRepository = recipeRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<Collection> CreateCollectionAsync(UpdateCollectionDto dto) {
            var entity = new Collection(dto.Name, dto.Description, CurrentSubjectId);

            if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl)) {
                entity.SetCoverImage(dto.CoverImageUrl);
            }

            if (dto.IsPublic.HasValue) {
                entity.SetVisibility(dto.IsPublic.Value);
            }

            using (logger.PushProperty("CollectionResourceId", entity.CollectionResourceId)) {
                await collectionRepository.AddAsync(entity).ConfigureAwait(false);
                logger.LogInformation("Created new collection");
                return entity;
            }
        }

        public async Task<Collection> GetCollectionAsync(Guid collectionResourceId) {
            var entity = await collectionRepository.GetAsync(collectionResourceId).ConfigureAwait(false);
            if (entity == null) {
                throw new CollectionNotFoundException($"Collection with id {collectionResourceId} not found");
            }

            // Check if user owns the collection or if it's public
            if (entity.SubjectId != CurrentSubjectId && !entity.IsPublic) {
                throw new CollectionNotFoundException($"Collection with id {collectionResourceId} not found");
            }

            return entity;
        }

        public Task<PagedList<Collection>> SearchCollectionsAsync(CollectionSearch search) {
            // Ensure users only see their own collections (unless searching public)
            if (!search.IsPublic.HasValue || !search.IsPublic.Value) {
                search.SubjectId = CurrentSubjectId;
            }
            return collectionRepository.SearchAsync(search);
        }

        public async Task<Collection> UpdateCollectionAsync(Guid resourceId, UpdateCollectionDto dto) {
            var entity = await GetCollectionAsync(resourceId).ConfigureAwait(false);

            // Check if user owns the collection
            if (entity.SubjectId != CurrentSubjectId) {
                throw new UnauthorizedAccessException("You can only update your own collections");
            }

            using (logger.PushProperty("CollectionResourceId", entity.CollectionResourceId)) {
                entity.Update(dto.Name, dto.Description);

                if (!string.IsNullOrWhiteSpace(dto.CoverImageUrl)) {
                    entity.SetCoverImage(dto.CoverImageUrl);
                }

                if (dto.IsPublic.HasValue) {
                    entity.SetVisibility(dto.IsPublic.Value);
                }

                logger.LogInformation("Updated collection");
                return entity;
            }
        }

        public async Task DeleteCollectionAsync(Guid resourceId) {
            var entity = await GetCollectionAsync(resourceId).ConfigureAwait(false);

            // Check if user owns the collection
            if (entity.SubjectId != CurrentSubjectId) {
                throw new UnauthorizedAccessException("You can only delete your own collections");
            }

            using (logger.PushProperty("CollectionResourceId", entity.CollectionResourceId)) {
                await collectionRepository.RemoveAsync(entity).ConfigureAwait(false);
                logger.LogInformation("Deleted collection");
            }
        }

        public async Task<Collection> AddRecipeToCollectionAsync(Guid collectionResourceId, Guid recipeResourceId, int? sortOrder) {
            var collection = await GetCollectionAsync(collectionResourceId).ConfigureAwait(false);

            // Check if user owns the collection
            if (collection.SubjectId != CurrentSubjectId) {
                throw new UnauthorizedAccessException("You can only add recipes to your own collections");
            }

            var recipe = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            // Check if user owns the recipe or if it's public
            if (recipe.CreatedSubject?.SubjectId != CurrentSubjectId && !recipe.IsPublic) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            // Determine sort order (default to end of list)
            var order = sortOrder ?? (collection.CollectionRecipes.Any() 
                ? collection.CollectionRecipes.Max(cr => cr.SortOrder) + 1 
                : 0);

            using (logger.PushProperty("CollectionResourceId", collection.CollectionResourceId)) {
                collection.AddRecipe(recipe.RecipeId, order);
                logger.LogInformation("Added recipe {RecipeResourceId} to collection", recipeResourceId);
                return collection;
            }
        }

        public async Task<Collection> RemoveRecipeFromCollectionAsync(Guid collectionResourceId, Guid recipeResourceId) {
            var collection = await GetCollectionAsync(collectionResourceId).ConfigureAwait(false);

            // Check if user owns the collection
            if (collection.SubjectId != CurrentSubjectId) {
                throw new UnauthorizedAccessException("You can only remove recipes from your own collections");
            }

            var recipe = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            var collectionRecipe = collection.CollectionRecipes.FirstOrDefault(cr => cr.RecipeId == recipe.RecipeId);
            if (collectionRecipe == null) {
                throw new InvalidOperationException("Recipe is not in this collection");
            }

            using (logger.PushProperty("CollectionResourceId", collection.CollectionResourceId)) {
                collection.RemoveRecipe(collectionRecipe);
                logger.LogInformation("Removed recipe {RecipeResourceId} from collection", recipeResourceId);
                return collection;
            }
        }

        public async Task<Collection> ReorderCollectionRecipesAsync(Guid collectionResourceId, Dictionary<Guid, int> recipeResourceIdToSortOrder) {
            var collection = await GetCollectionAsync(collectionResourceId).ConfigureAwait(false);

            // Check if user owns the collection
            if (collection.SubjectId != CurrentSubjectId) {
                throw new UnauthorizedAccessException("You can only reorder recipes in your own collections");
            }

            // Convert resource IDs to internal IDs
            var recipeIdToSortOrder = new Dictionary<int, int>();
            foreach (var kvp in recipeResourceIdToSortOrder) {
                var recipe = await recipeRepository.GetAsync(kvp.Key).ConfigureAwait(false);
                if (recipe != null) {
                    recipeIdToSortOrder[recipe.RecipeId] = kvp.Value;
                }
            }

            using (logger.PushProperty("CollectionResourceId", collection.CollectionResourceId)) {
                collection.ReorderRecipes(recipeIdToSortOrder);
                logger.LogInformation("Reordered recipes in collection");
                return collection;
            }
        }

        public async Task ReorderCollectionsAsync(List<(Guid CollectionResourceId, int SortOrder)> collectionOrders) {
            foreach (var (collectionResourceId, sortOrder) in collectionOrders) {
                var collection = await collectionRepository.GetAsync(collectionResourceId).ConfigureAwait(false);
                if (collection != null && collection.SubjectId == CurrentSubjectId) {
                    collection.SetSortOrder(sortOrder);
                }
            }

            logger.LogInformation("Reordered {Count} collections", collectionOrders.Count);
        }

        public Task<List<Collection>> GetUserCollectionsAsync() {
            return collectionRepository.GetUserCollectionsAsync(CurrentSubjectId);
        }

        public Task<PagedList<Collection>> GetPublicCollectionsAsync(string searchTerm, int pageNumber, int pageSize) {
            return collectionRepository.GetPublicCollectionsAsync(searchTerm, pageNumber, pageSize);
        }

        public Task<PagedList<Collection>> GetFeaturedCollectionsAsync(int pageNumber, int pageSize) {
            return collectionRepository.GetFeaturedCollectionsAsync(pageNumber, pageSize);
        }
    }
}
