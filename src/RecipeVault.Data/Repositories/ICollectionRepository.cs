using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface ICollectionRepository {
        Task<Collection> AddAsync(Collection collection);
        Task<Collection> GetAsync(Guid collectionResourceId);
        Task<Collection> GetByIdAsync(int collectionId);
        Task<PagedList<Collection>> SearchAsync(CollectionSearch search);
        Task RemoveAsync(Collection collection);
        Task<List<Collection>> GetUserCollectionsAsync(Guid subjectId);
        Task<PagedList<Collection>> GetPublicCollectionsAsync(string searchTerm, int pageNumber, int pageSize);
        Task<PagedList<Collection>> GetFeaturedCollectionsAsync(int pageNumber, int pageSize);
    }
}
