using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class UnitRepository : IUnitRepository {
        private readonly IRecipeVaultDbContext context;

        public UnitRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<Unit>> GetAllWithAliasesAsync() {
            return await context.Units
                .Include(u => u.Aliases)
                .Where(u => u.IsActive)
                .OrderBy(u => u.SortOrder)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public Task<Unit> GetByIdAsync(Guid resourceId) {
            return context.Units
                .Include(u => u.Aliases)
                .FirstOrDefaultAsync(u => u.UnitResourceId == resourceId);
        }

        public Task<Unit> GetByIdAsync(int unitId) {
            return context.Units
                .Include(u => u.Aliases)
                .FirstOrDefaultAsync(u => u.UnitId == unitId);
        }
    }
}
