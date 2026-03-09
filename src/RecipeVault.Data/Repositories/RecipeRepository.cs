using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class RecipeRepository : IRecipeRepository {
        private readonly IRecipeVaultDbContext context;

        public RecipeRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<Recipe>> SearchAsync(RecipeSearch model) {
            var recipes = model.Build(context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.Instructions)
                .Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .Include(x => x.ForkedFromRecipe)
                    .ThenInclude(x => x.CreatedSubject));

            var result = new PagedList<Recipe> {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                TotalItems = await recipes.CountAsync().ConfigureAwait(false),
                Items = [],
            };

            if (!string.IsNullOrEmpty(model.SortBy)) {
                bool descending = model.SortDirection?.ToLowerInvariant() != "asc";
                recipes = model.SortBy switch {
                    "ForkCount" => descending
                        ? recipes.OrderByDescending(x => x.ForkCount)
                        : recipes.OrderBy(x => x.ForkCount),
                    "CreatedDate" => descending
                        ? recipes.OrderByDescending(x => x.CreatedDate)
                        : recipes.OrderBy(x => x.CreatedDate),
                    "Rating" => descending
                        ? recipes.OrderByDescending(x => x.Rating)
                        : recipes.OrderBy(x => x.Rating),
                    _ => recipes.ToSortedQuery(model.Sort)
                };
            } else {
                recipes = recipes.ToSortedQuery(model.Sort);
            }
            result.Items = await recipes.ToPagedQuery(model.PageNumber, model.PageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<Recipe> AddAsync(Recipe recipe) {
            var entity = await context.Recipes.AddAsync(recipe);
            return entity.Entity;
        }

        public Task<Recipe> GetAsync(Guid id) {
            return context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.Instructions)
                .Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .Include(x => x.ForkedFromRecipe)
                    .ThenInclude(x => x.CreatedSubject)
                .FirstOrDefaultAsync(r => r.RecipeResourceId == id);
        }

        public Task<Recipe> GetByIdAsync(int recipeId) {
            return context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag)
                .Include(x => x.CreatedSubject)
                .Include(x => x.ForkedFromRecipe)
                    .ThenInclude(x => x.CreatedSubject)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);
        }

        public Task RemoveAsync(Recipe recipe) {
            context.Remove(recipe);
            return Task.CompletedTask;
        }

        public Task<Recipe> GetByShareTokenAsync(string shareToken) {
            return context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.Instructions)
                .Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .Include(x => x.ForkedFromRecipe)
                    .ThenInclude(x => x.CreatedSubject)
                .FirstOrDefaultAsync(r => r.ShareToken == shareToken);
        }
    }
}
