using System;
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
                .Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag).ThenInclude(t => t.UserTagAliases)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject));

            var result = new PagedList<Recipe> {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                TotalItems = await recipes.CountAsync().ConfigureAwait(false),
                Items = [],
            };

            recipes = recipes.ToSortedQuery(model.Sort);
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
                .FirstOrDefaultAsync(r => r.RecipeResourceId == id);
        }

        public Task<Recipe> GetByIdAsync(int recipeId) {
            return context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.RecipeTags).ThenInclude(rt => rt.Tag)
                .Include(x => x.CreatedSubject)
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
                .FirstOrDefaultAsync(r => r.ShareToken == shareToken);
        }
    }
}
