using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;

namespace RecipeVault.DomainService {
    public class UnitService : IUnitService {
        private readonly IUnitRepository unitRepository;
        private readonly IMemoryCache cache;
        private readonly ILogger<UnitService> logger;
        private static readonly string AllUnitsCacheKey = "all_units";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public UnitService(IUnitRepository unitRepository, IMemoryCache cache, ILogger<UnitService> logger) {
            this.unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UnitMatchResult> MatchAsync(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return UnitMatchResult.NoMatch(input);
            }

            var normalized = input.Trim().ToLowerInvariant();

            // Check match cache
            var matchCacheKey = $"unit_match_{normalized}";
            if (cache.TryGetValue(matchCacheKey, out UnitMatchResult cached)) {
                logger.LogDebug("Unit match cache hit for '{Input}'", input);
                return cached;
            }

            // Get all units (cached)
            var allUnits = await GetAllUnitsWithAliasesAsync().ConfigureAwait(false);

            // 1. Exact match on name, abbreviation, or plural name
            var exactMatch = allUnits.FirstOrDefault(u =>
                u.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                u.Abbreviation.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                (u.PluralName != null && u.PluralName.Equals(normalized, StringComparison.OrdinalIgnoreCase)));

            if (exactMatch != null) {
                var result = UnitMatchResult.ExactMatch(exactMatch, input);
                cache.Set(matchCacheKey, result, CacheDuration);
                logger.LogDebug("Exact unit match for '{Input}' -> '{Unit}'", input, exactMatch.Name);
                return result;
            }

            // 2. Exact match on alias
            foreach (var unit in allUnits) {
                var aliasMatch = unit.Aliases.FirstOrDefault(a =>
                    a.Alias.Equals(normalized, StringComparison.OrdinalIgnoreCase));

                if (aliasMatch != null) {
                    var result = UnitMatchResult.ExactMatch(unit, input);
                    cache.Set(matchCacheKey, result, CacheDuration);
                    logger.LogDebug("Alias unit match for '{Input}' -> '{Unit}' (via alias '{Alias}')", input, unit.Name, aliasMatch.Alias);
                    return result;
                }
            }

            // 3. Fuzzy match (Levenshtein distance <= 2)
            var fuzzyMatch = FindFuzzyMatch(normalized, allUnits);
            if (fuzzyMatch != null) {
                cache.Set(matchCacheKey, fuzzyMatch, CacheDuration);
                logger.LogDebug("Fuzzy unit match for '{Input}' -> '{Unit}' (confidence: {Confidence})", input, fuzzyMatch.MatchedUnit.Name, fuzzyMatch.Confidence);
                return fuzzyMatch;
            }

            // No match found
            logger.LogDebug("No unit match found for '{Input}'", input);
            return UnitMatchResult.NoMatch(input);
        }

        public async Task<IReadOnlyList<Unit>> GetAllAsync() {
            return await GetAllUnitsWithAliasesAsync().ConfigureAwait(false);
        }

        public Task<Unit> GetByIdAsync(Guid resourceId) {
            return unitRepository.GetByIdAsync(resourceId);
        }

        private async Task<IReadOnlyList<Unit>> GetAllUnitsWithAliasesAsync() {
            if (cache.TryGetValue(AllUnitsCacheKey, out IReadOnlyList<Unit> cachedUnits)) {
                return cachedUnits;
            }

            var units = await unitRepository.GetAllWithAliasesAsync().ConfigureAwait(false);
            cache.Set(AllUnitsCacheKey, units, CacheDuration);
            return units;
        }

        private static UnitMatchResult FindFuzzyMatch(string input, IReadOnlyList<Unit> units) {
            UnitMatchResult bestMatch = null;

            foreach (var unit in units) {
                // Candidates: name, abbreviation, plural name, and all aliases
                var candidates = new[] { unit.Name, unit.Abbreviation, unit.PluralName }
                    .Concat(unit.Aliases.Select(a => a.Alias))
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var candidate in candidates) {
                    var distance = LevenshteinDistance(input, candidate.ToLowerInvariant());

                    // Only consider matches with distance <= 2
                    if (distance <= 2 && distance > 0) {
                        // Confidence decreases with distance: 0.9 for dist 1, 0.8 for dist 2
                        var confidence = 1.0m - (distance * 0.1m);

                        if (bestMatch == null || confidence > bestMatch.Confidence) {
                            bestMatch = UnitMatchResult.FuzzyMatch(unit, input, confidence);
                        }
                    }
                }
            }

            return bestMatch;
        }

        private static int LevenshteinDistance(string a, string b) {
            if (string.IsNullOrEmpty(a)) {
                return string.IsNullOrEmpty(b) ? 0 : b.Length;
            }

            if (string.IsNullOrEmpty(b)) {
                return a.Length;
            }

            var lengthA = a.Length;
            var lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];

            for (var i = 0; i <= lengthA; i++) {
                distances[i, 0] = i;
            }

            for (var j = 0; j <= lengthB; j++) {
                distances[0, j] = j;
            }

            for (var i = 1; i <= lengthA; i++) {
                for (var j = 1; j <= lengthB; j++) {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost);
                }
            }

            return distances[lengthA, lengthB];
        }
    }
}
