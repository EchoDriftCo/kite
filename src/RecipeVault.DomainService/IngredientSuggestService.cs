using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public class IngredientSuggestService : IIngredientSuggestService {
        private readonly ILogger<IngredientSuggestService> logger;
        private readonly string connectionString;

        public IngredientSuggestService(
            ILogger<IngredientSuggestService> logger,
            IConfiguration configuration) {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            connectionString = configuration["Database:ConnectionString"];
        }

        public async Task<List<IngredientSuggestionDto>> SuggestAsync(string query, int limit = 10) {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2) {
                return new List<IngredientSuggestionDto>();
            }

            var hasPgTrgm = await CheckPgTrgmAvailableAsync().ConfigureAwait(false);

            var sql = hasPgTrgm
                ? BuildTrigramSuggestQuery()
                : BuildILikeSuggestQuery();

            var suggestions = new List<IngredientSuggestionDto>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            await using var command = conn.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Query", query.ToLowerInvariant());
            command.Parameters.AddWithValue("@Limit", limit);

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false)) {
                suggestions.Add(new IngredientSuggestionDto {
                    Name = reader.GetString(0),
                    RecipeCount = reader.GetInt32(1)
                });
            }

            return suggestions;
        }

        internal async Task<bool> CheckPgTrgmAvailableAsync() {
            var sql = "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm');";
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            await using var command = conn.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            return (bool)(result ?? false);
        }

        private static string BuildTrigramSuggestQuery() {
            return @"
                SELECT
                    LOWER(TRIM(ri.""Item"")) AS ingredient_name,
                    COUNT(DISTINCT ri.""RecipeId"") AS recipe_count
                FROM ""RecipeIngredient"" ri
                WHERE
                    ri.""Item"" IS NOT NULL
                    AND TRIM(ri.""Item"") <> ''
                    AND LOWER(ri.""Item"") % @Query
                GROUP BY ingredient_name
                ORDER BY
                    similarity(LOWER(ri.""Item""), @Query) DESC,
                    recipe_count DESC
                LIMIT @Limit;
            ";
        }

        private static string BuildILikeSuggestQuery() {
            return @"
                SELECT
                    LOWER(TRIM(ri.""Item"")) AS ingredient_name,
                    COUNT(DISTINCT ri.""RecipeId"") AS recipe_count
                FROM ""RecipeIngredient"" ri
                WHERE
                    ri.""Item"" IS NOT NULL
                    AND TRIM(ri.""Item"") <> ''
                    AND LOWER(ri.""Item"") ILIKE @Query || '%'
                GROUP BY ingredient_name
                ORDER BY recipe_count DESC
                LIMIT @Limit;
            ";
        }
    }
}
