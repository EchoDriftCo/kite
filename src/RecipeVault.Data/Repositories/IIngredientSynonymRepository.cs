using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IIngredientSynonymRepository {
        Task<List<IngredientSynonym>> GetAllAsync();
        Task<List<IngredientSynonym>> GetByCanonicalNameAsync(string canonicalName);
        Task<IngredientSynonym> GetBySynonymAsync(string synonym);
    }
}
