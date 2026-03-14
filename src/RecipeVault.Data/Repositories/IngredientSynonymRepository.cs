using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class IngredientSynonymRepository : IIngredientSynonymRepository {
        private readonly IRecipeVaultDbContext context;

        public IngredientSynonymRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<IngredientSynonym>> GetAllAsync() {
            return context.IngredientSynonyms
                .OrderBy(s => s.CanonicalName)
                .ThenBy(s => s.Synonym)
                .ToListAsync();
        }

        public Task<List<IngredientSynonym>> GetByCanonicalNameAsync(string canonicalName) {
            return context.IngredientSynonyms
                .Where(s => s.CanonicalName == canonicalName)
                .OrderBy(s => s.Synonym)
                .ToListAsync();
        }

        public Task<IngredientSynonym> GetBySynonymAsync(string synonym) {
            return context.IngredientSynonyms
                .FirstOrDefaultAsync(s => s.Synonym == synonym);
        }
    }
}
