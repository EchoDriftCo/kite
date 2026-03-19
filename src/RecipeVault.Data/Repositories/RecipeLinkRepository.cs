using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class RecipeLinkRepository : IRecipeLinkRepository {
        private readonly IRecipeVaultDbContext context;

        public RecipeLinkRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RecipeLink> AddAsync(RecipeLink recipeLink) {
            var entity = await context.RecipeLinks.AddAsync(recipeLink);
            return entity.Entity;
        }

        public Task<RecipeLink> GetAsync(Guid recipeLinkResourceId) {
            return context.RecipeLinks
                .Include(rl => rl.ParentRecipe)
                    .ThenInclude(r => r.CreatedSubject)
                .Include(rl => rl.LinkedRecipe)
                    .ThenInclude(r => r.CreatedSubject)
                .Include(rl => rl.CreatedSubject)
                .Include(rl => rl.LastModifiedSubject)
                .FirstOrDefaultAsync(rl => rl.RecipeLinkResourceId == recipeLinkResourceId);
        }

        public Task<RecipeLink> GetByIdAsync(int recipeLinkId) {
            return context.RecipeLinks
                .Include(rl => rl.ParentRecipe)
                .Include(rl => rl.LinkedRecipe)
                .Include(rl => rl.CreatedSubject)
                .FirstOrDefaultAsync(rl => rl.RecipeLinkId == recipeLinkId);
        }

        public async Task<List<RecipeLink>> GetLinkedRecipesAsync(int parentRecipeId) {
            return await context.RecipeLinks
                .Include(rl => rl.LinkedRecipe)
                    .ThenInclude(r => r.Ingredients)
                .Include(rl => rl.LinkedRecipe)
                    .ThenInclude(r => r.Instructions)
                .Include(rl => rl.LinkedRecipe)
                    .ThenInclude(r => r.RecipeTags)
                        .ThenInclude(rt => rt.Tag)
                .Include(rl => rl.LinkedRecipe)
                    .ThenInclude(r => r.CreatedSubject)
                .Where(rl => rl.ParentRecipeId == parentRecipeId)
                .ToListAsync();
        }

        public async Task<List<RecipeLink>> GetUsedInRecipesAsync(int linkedRecipeId) {
            return await context.RecipeLinks
                .Include(rl => rl.ParentRecipe)
                    .ThenInclude(r => r.CreatedSubject)
                .Where(rl => rl.LinkedRecipeId == linkedRecipeId)
                .ToListAsync();
        }

        public Task RemoveAsync(RecipeLink recipeLink) {
            context.Remove(recipeLink);
            return Task.CompletedTask;
        }

        public async Task<bool> HasCircularLinkAsync(int parentRecipeId, int linkedRecipeId) {
            // Check if linkedRecipeId links (directly or indirectly) to parentRecipeId
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(linkedRecipeId);

            while (queue.Count > 0) {
                var currentId = queue.Dequeue();
                if (currentId == parentRecipeId) {
                    return true; // Circular link detected
                }

                if (visited.Contains(currentId)) {
                    continue;
                }

                visited.Add(currentId);

                // Get all recipes that currentId links to
                var childLinks = await context.RecipeLinks
                    .Where(rl => rl.ParentRecipeId == currentId)
                    .Select(rl => rl.LinkedRecipeId)
                    .ToListAsync();

                foreach (var childId in childLinks) {
                    queue.Enqueue(childId);
                }
            }

            return false;
        }
    }
}
