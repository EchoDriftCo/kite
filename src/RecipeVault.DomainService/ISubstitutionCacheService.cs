using System;
using System.Threading.Tasks;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for caching substitution analysis results
    /// </summary>
    public interface ISubstitutionCacheService {
        /// <summary>
        /// Get cached substitution result
        /// </summary>
        Task<T> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Set cached substitution result with TTL
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class;

        /// <summary>
        /// Build a cache key from substitution parameters
        /// </summary>
        string BuildCacheKey(Guid recipeResourceId, System.Collections.Generic.List<int> ingredientIndices, System.Collections.Generic.List<string> dietaryConstraints);
    }
}
