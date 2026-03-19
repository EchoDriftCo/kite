using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface ICollectionFacade {
        Task<CollectionDto> CreateCollectionAsync(UpdateCollectionDto dto);
        Task<CollectionDto> GetCollectionAsync(Guid resourceId);
        Task<PagedList<CollectionDto>> SearchCollectionsAsync(CollectionSearchDto search);
        Task<CollectionDto> UpdateCollectionAsync(Guid resourceId, UpdateCollectionDto dto);
        Task DeleteCollectionAsync(Guid resourceId);
        Task<CollectionDto> AddRecipeToCollectionAsync(Guid collectionResourceId, AddRecipeToCollectionDto dto);
        Task<CollectionDto> RemoveRecipeFromCollectionAsync(Guid collectionResourceId, Guid recipeResourceId);
        Task<CollectionDto> ReorderCollectionRecipesAsync(Guid collectionResourceId, ReorderCollectionRecipesDto dto);
        Task ReorderCollectionsAsync(ReorderCollectionsDto dto);
        Task<List<CollectionDto>> GetUserCollectionsAsync();
        Task<PagedList<CollectionDto>> GetPublicCollectionsAsync(string searchTerm, int pageNumber, int pageSize);
        Task<PagedList<CollectionDto>> GetFeaturedCollectionsAsync(int pageNumber, int pageSize);
    }
}
