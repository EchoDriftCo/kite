using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.DomainService {
    public interface ITagService {
        Task<Tag> CreateTagAsync(string name, TagCategory category);
        Task<Tag> GetOrCreateTagAsync(string name, TagCategory category);
        Task<Tag> GetTagAsync(Guid tagResourceId);
        Task<PagedList<Tag>> SearchTagsAsync(TagSearch search);
        Task<Tag> UpdateTagAsync(Guid tagResourceId, string name, TagCategory category);
        Task DeleteTagAsync(Guid tagResourceId);
    }
}
