using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Data.Repositories {
    public class TagRepository : ITagRepository {
        private readonly IRecipeVaultDbContext context;

        public TagRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Tag> AddAsync(Tag tag) {
            var entity = await context.Tags.AddAsync(tag);
            return entity.Entity;
        }

        public Task<Tag> GetAsync(Guid id) {
            return context.Tags
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(t => t.TagResourceId == id);
        }

        public Task<Tag> GetByNameAndCategoryAsync(string name, TagCategory category) {
            return context.Tags
                .Include(x => x.CreatedSubject)
                .FirstOrDefaultAsync(t => t.Name == name && t.Category == category);
        }

        public async Task<PagedList<Tag>> SearchAsync(TagSearch model) {
            var tags = model.Build(context.Tags
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject));

            var result = new PagedList<Tag> {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                TotalItems = await tags.CountAsync().ConfigureAwait(false),
                Items = [],
            };

            tags = tags.ToSortedQuery(model.Sort);
            result.Items = await tags.ToPagedQuery(model.PageNumber, model.PageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public Task RemoveAsync(Tag tag) {
            context.Remove(tag);
            return Task.CompletedTask;
        }
    }
}
