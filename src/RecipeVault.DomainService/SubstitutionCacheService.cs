using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Implementation of substitution caching using IMemoryCache
    /// </summary>
    public class SubstitutionCacheService : ISubstitutionCacheService {
        private readonly IMemoryCache cache;

        public SubstitutionCacheService(IMemoryCache cache) {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Get cached substitution result
        /// </summary>
        public Task<T> GetAsync<T>(string key) where T : class {
            cache.TryGetValue(key, out T value);
            return Task.FromResult(value);
        }

        /// <summary>
        /// Set cached substitution result with TTL
        /// </summary>
        public Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class {
            var options = new MemoryCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = ttl
            };
            cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Build a cache key from substitution parameters
        /// </summary>
        public string BuildCacheKey(Guid recipeResourceId, List<int> ingredientIndices, List<string> dietaryConstraints) {
            var data = new {
                recipeId = recipeResourceId.ToString(),
                indices = ingredientIndices?.OrderBy(x => x).ToList() ?? new List<int>(),
                constraints = dietaryConstraints?.OrderBy(x => x).ToList() ?? new List<string>()
            };

            var json = JsonSerializer.Serialize(data);
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            
            return $"substitution:{hash}";
        }
    }
}
