using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Data.Repositories {
    public interface ITagRepository {
        Task<Tag> AddAsync(Tag tag);
        Task<Tag> GetAsync(Guid id);
        Task<Tag> GetByNameAndCategoryAsync(string name, TagCategory category);
        Task<PagedList<Tag>> SearchAsync(TagSearch model);
        Task RemoveAsync(Tag tag);
    }
}
