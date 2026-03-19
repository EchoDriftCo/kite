using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
namespace RecipeVault.DomainService {
    public class IngredientSearchService : IIngredientSearchService {
        private readonly IRecipeRepository recipeRepository;
        private readonly IUserPantryRepository userPantryRepository;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly ILogger<IngredientSearchService> logger;
        private readonly DbContext dbContext;
        private readonly List<string> defaultPantryStaples;

        public IngredientSearchService(
            IRecipeRepository recipeRepository,
            IUserPantryRepository userPantryRepository,
            ISubjectPrincipal subjectPrincipal,
            ILogger<IngredientSearchService> logger,
            DbContext dbContext,
            IConfiguration configuration) {
            this.recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
            this.userPantryRepository = userPantryRepository ?? throw new ArgumentNullException(nameof(userPantryRepository));
            this.subjectPrincipal = subjectPrincipal ?? throw new ArgumentNullException(nameof(subjectPrincipal));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            defaultPantryStaples = configuration.GetSection("IngredientSearch:DefaultPantryStaples").Get<List<string>>()
                ?? new List<string> { "salt", "black pepper", "water", "olive oil", "vegetable oil", "butter", "all-purpose flour", "granulated sugar", "garlic", "onion" };
        }

        public async Task<List<IngredientSearchResultDto>> SearchByIngredientsAsync(IngredientSearchRequestDto request) {
            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);

            // 1. Seed user pantry if this is their first access
            await EnsureUserPantrySeededAsync(currentSubjectId).ConfigureAwait(false);

            // 2. Get user's pantry staples if requested
            var pantryStaples = request.IncludePantryStaples
                ? await userPantryRepository.GetStaplesAsync(currentSubjectId).ConfigureAwait(false)
                : new List<string>();

            // 3. Call PostgreSQL scoring function
            var scored = await ScoreRecipesInDatabaseAsync(
                currentSubjectId,
                request.Ingredients,
                pantryStaples
            ).ConfigureAwait(false);

            // 4. Apply filters
            var filtered = scored
                .Where(r => r.MissingIngredients.Count <= request.MaxMissingIngredients)
                .ToList();

            if (request.MaxCookTimeMinutes.HasValue) {
                filtered = filtered
                    .Where(r => r.Recipe.TotalTimeMinutes.HasValue && r.Recipe.TotalTimeMinutes <= request.MaxCookTimeMinutes.Value)
                    .ToList();
            }

            // TODO: dietary profile filtering will be implemented in a future iteration

            // 5. Sort
            var sorted = request.SortBy switch {
                "matchPercentage" => filtered
                    .OrderByDescending(r => r.WeightedMatchPercentage)
                    .ThenByDescending(r => r.Recipe.Rating ?? 0),
                "cookTime" => filtered
                    .OrderBy(r => r.Recipe.TotalTimeMinutes ?? int.MaxValue),
                "rating" => filtered
                    .OrderByDescending(r => r.Recipe.Rating ?? 0),
                _ => filtered
                    .OrderByDescending(r => r.WeightedMatchPercentage)
            };

            return sorted.ToList();
        }

        private async Task<List<IngredientSearchResultDto>> ScoreRecipesInDatabaseAsync(
            Guid subjectId,
            List<string> userIngredients,
            List<string> pantryStaples) {
            var sql = @"
                SELECT
                    recipe_id,
                    matched_ingredients,
                    missing_ingredients,
                    pantry_staples_used,
                    match_percentage,
                    weighted_match_percentage
                FROM score_recipes_by_ingredients(
                    @SubjectId,
                    @UserIngredients,
                    @PantryStaples
                );
            ";

            var results = new List<IngredientSearchResultDto>();

            var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) {
                await conn.OpenAsync().ConfigureAwait(false);
            }
            await using var command = conn.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@SubjectId", NpgsqlDbType.Uuid, subjectId);
            command.Parameters.AddWithValue("@UserIngredients", NpgsqlDbType.Array | NpgsqlDbType.Text, userIngredients.ToArray());
            command.Parameters.AddWithValue("@PantryStaples", NpgsqlDbType.Array | NpgsqlDbType.Text, pantryStaples.ToArray());

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            var recipeIds = new List<(int RecipeId, string[] Matched, string[] Missing, string[] Pantry, decimal MatchPct, decimal WeightedPct)>();

            while (await reader.ReadAsync().ConfigureAwait(false)) {
                recipeIds.Add((
                    reader.GetInt32(0),
                    reader.GetFieldValue<string[]>(1),
                    reader.GetFieldValue<string[]>(2),
                    reader.GetFieldValue<string[]>(3),
                    reader.GetDecimal(4),
                    reader.GetDecimal(5)
                ));
            }

            // Batch load all scored recipes to avoid N+1 queries
            var ids = recipeIds.Select(r => r.RecipeId).ToList();
            var recipes = await recipeRepository.GetByIdsAsync(ids).ConfigureAwait(false);
            var recipeLookup = recipes.ToDictionary(r => r.RecipeId);

            foreach (var scored in recipeIds) {
                if (recipeLookup.TryGetValue(scored.RecipeId, out var recipe)) {
                    results.Add(new IngredientSearchResultDto {
                        Recipe = MapToRecipeSummary(recipe, subjectId),
                        MatchedIngredients = scored.Matched.ToList(),
                        MissingIngredients = scored.Missing.ToList(),
                        PantryStaplesUsed = scored.Pantry.ToList(),
                        MatchPercentage = (double)scored.MatchPct,
                        WeightedMatchPercentage = (double)scored.WeightedPct,
                        SubstitutionsAvailable = new List<SubstitutionSuggestionDto>()
                    });
                }
            }

            return results;
        }

        private async Task EnsureUserPantrySeededAsync(Guid subjectId) {
            var existingCount = await userPantryRepository.CountAsync(subjectId).ConfigureAwait(false);

            if (existingCount == 0) {
                var stapleItems = defaultPantryStaples.Select(name => new UserPantryItem(
                    subjectId,
                    name,
                    isStaple: true
                )).ToList();

                await userPantryRepository.AddRangeAsync(stapleItems).ConfigureAwait(false);

                logger.LogInformation(
                    "Seeded {Count} default pantry staples for user {SubjectId}",
                    stapleItems.Count,
                    subjectId
                );
            }
        }

        private static RecipeSummaryDto MapToRecipeSummary(Recipe recipe, Guid currentSubjectId) {
            var ownerSubjectId = recipe.CreatedSubject?.SubjectId;

            return new RecipeSummaryDto {
                RecipeResourceId = recipe.RecipeResourceId,
                Title = recipe.Title,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                TotalTimeMinutes = recipe.TotalTimeMinutes,
                OriginalImageUrl = recipe.OriginalImageUrl,
                Rating = recipe.Rating,
                IsFavorite = recipe.IsFavorite,
                IsOwner = ownerSubjectId.HasValue && ownerSubjectId == currentSubjectId,
                Tags = recipe.RecipeTags?
                    .Where(rt => !rt.IsOverridden)
                    .Select(rt => new RecipeTagDto {
                        TagResourceId = rt.Tag?.TagResourceId ?? Guid.Empty,
                        GlobalName = rt.Tag?.Name,
                        DisplayName = rt.Tag?.Name,
                        Category = (int)(rt.Tag?.Category ?? 0),
                        IsAiAssigned = rt.IsAiAssigned,
                        Confidence = rt.Confidence
                    }).ToList() ?? new List<RecipeTagDto>(),
                Ingredients = recipe.Ingredients?
                    .Select(i => new RecipeIngredientDto {
                        RecipeIngredientId = i.RecipeIngredientId,
                        SortOrder = i.SortOrder,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Item = i.Item,
                        Preparation = i.Preparation,
                        RawText = i.RawText
                    }).ToList() ?? new List<RecipeIngredientDto>()
            };
        }
    }
}
