using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class CollectionRepository : ICollectionRepository {
        private readonly IRecipeVaultDbContext dbContext;

        public CollectionRepository(IRecipeVaultDbContext dbContext) {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Collection> AddAsync(Collection collection) {
            var entity = await dbContext.Collections.AddAsync(collection).ConfigureAwait(false);
            return entity.Entity;
        }

        public Task<Collection> GetAsync(Guid collectionResourceId) {
            return dbContext.Collections
                .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                .FirstOrDefaultAsync(c => c.CollectionResourceId == collectionResourceId);
        }

        public Task<Collection> GetByIdAsync(int collectionId) {
            return dbContext.Collections
                .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                .FirstOrDefaultAsync(c => c.CollectionId == collectionId);
        }

        public async Task<PagedList<Collection>> SearchAsync(CollectionSearch search) {
            var query = search.Build(dbContext.Collections
                .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe));

            var result = new PagedList<Collection> {
                PageNumber = search.PageNumber,
                PageSize = search.PageSize,
                TotalItems = await query.CountAsync().ConfigureAwait(false),
                Items = []
            };

            query = query.ToSortedQuery(search.Sort);
            result.Items = await query.ToPagedQuery(search.PageNumber, search.PageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public Task RemoveAsync(Collection collection) {
            dbContext.RemoveRange(collection.CollectionRecipes);
            dbContext.Remove(collection);
            return Task.CompletedTask;
        }

        public Task<List<Collection>> GetUserCollectionsAsync(Guid subjectId) {
            return dbContext.Collections
                .Where(c => c.SubjectId == subjectId)
                .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<PagedList<Collection>> GetPublicCollectionsAsync(string searchTerm, int pageNumber, int pageSize) {
            var query = dbContext.Collections
                .Where(c => c.IsPublic)
                .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm)) {
                query = query.Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm));
            }

            query = query.OrderBy(c => c.Name);

            var result = new PagedList<Collection> {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = await query.CountAsync().ConfigureAwait(false),
                Items = []
            };

            result.Items = await query.ToPagedQuery(pageNumber, pageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<PagedList<Collection>> GetFeaturedCollectionsAsync(int pageNumber, int pageSize) {
            var query = dbContext.Collections
                .Where(c => c.IsFeatured)
                .Include(c => c.CollectionRecipes)
                .ThenInclude(cr => cr.Recipe)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name);

            var result = new PagedList<Collection> {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = await query.CountAsync().ConfigureAwait(false),
                Items = []
            };

            result.Items = await query.ToPagedQuery(pageNumber, pageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }
    }
}
