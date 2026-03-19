using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public interface ICollectionService {
        Task<Collection> CreateCollectionAsync(UpdateCollectionDto dto);
        Task<Collection> GetCollectionAsync(Guid collectionResourceId);
        Task<PagedList<Collection>> SearchCollectionsAsync(CollectionSearch search);
        Task<Collection> UpdateCollectionAsync(Guid resourceId, UpdateCollectionDto dto);
        Task DeleteCollectionAsync(Guid resourceId);
        Task<Collection> AddRecipeToCollectionAsync(Guid collectionResourceId, Guid recipeResourceId, int? sortOrder);
        Task<Collection> RemoveRecipeFromCollectionAsync(Guid collectionResourceId, Guid recipeResourceId);
        Task<Collection> ReorderCollectionRecipesAsync(Guid collectionResourceId, Dictionary<Guid, int> recipeResourceIdToSortOrder);
        Task ReorderCollectionsAsync(List<(Guid CollectionResourceId, int SortOrder)> collectionOrders);
        Task<List<Collection>> GetUserCollectionsAsync();
        Task<PagedList<Collection>> GetPublicCollectionsAsync(string searchTerm, int pageNumber, int pageSize);
        Task<PagedList<Collection>> GetFeaturedCollectionsAsync(int pageNumber, int pageSize);
    }
}
