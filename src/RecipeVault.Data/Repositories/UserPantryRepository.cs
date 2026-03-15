using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class UserPantryRepository : IUserPantryRepository {
        private readonly IRecipeVaultDbContext context;

        public UserPantryRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<UserPantryItem>> GetBySubjectIdAsync(Guid subjectId) {
            return context.UserPantryItems
                .Where(p => p.SubjectId == subjectId)
                .OrderBy(p => p.IngredientName)
                .ToListAsync();
        }

        public Task<UserPantryItem> GetByIdAsync(int id) {
            return context.UserPantryItems
                .FirstOrDefaultAsync(p => p.UserPantryItemId == id);
        }

        public Task<List<string>> GetStaplesAsync(Guid subjectId) {
            return context.UserPantryItems
                .Where(p => p.SubjectId == subjectId && p.IsStaple)
                .Select(p => p.IngredientName)
                .ToListAsync();
        }

        public Task<int> CountAsync(Guid subjectId) {
            return context.UserPantryItems
                .CountAsync(p => p.SubjectId == subjectId);
        }

        public async Task<UserPantryItem> AddAsync(UserPantryItem item) {
            var entity = await context.UserPantryItems.AddAsync(item);
            return entity.Entity;
        }

        public async Task AddRangeAsync(List<UserPantryItem> items) {
            await context.UserPantryItems.AddRangeAsync(items);
        }

        public void Remove(UserPantryItem item) {
            context.Remove(item);
        }
    }
}
