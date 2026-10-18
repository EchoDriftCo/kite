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
        /// <summary>
        /// Finds a global tag by name and category, or, when <paramref name="subjectId"/> is given, that
        /// user's own tag. Never another user's private tag.
        /// </summary>
        Task<Tag> GetByNameAndCategoryAsync(string name, TagCategory category, Guid? subjectId);
        Task<PagedList<Tag>> SearchAsync(TagSearch model);
        Task RemoveAsync(Tag tag);
    }
}
