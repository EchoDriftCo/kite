using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    public class MealPlanService : IMealPlanService {
        private readonly ILogger<MealPlanService> logger;
        private readonly IMealPlanRepository mealPlanRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly IGeminiClient geminiClient;

        public MealPlanService(IMealPlanRepository mealPlanRepository, IRecipeRepository recipeRepository, ILogger<MealPlanService> logger, ISubjectPrincipal subjectPrincipal, IGeminiClient geminiClient) {
            this.logger = logger;
            this.mealPlanRepository = mealPlanRepository;
            this.recipeRepository = recipeRepository;
            this.subjectPrincipal = subjectPrincipal;
            this.geminiClient = geminiClient;
        }

        public async Task<MealPlan> CreateMealPlanAsync(UpdateMealPlanDto dto) {
            var entity = new MealPlan(dto.Name, dto.StartDate, dto.EndDate);

            if (dto.Entries != null) {
                await SetEntriesFromDtoAsync(entity, dto.Entries).ConfigureAwait(false);
            }

            using (logger.PushProperty("MealPlanResourceId", entity.MealPlanResourceId)) {
                await mealPlanRepository.AddAsync(entity);
                logger.LogInformation("Created new meal plan");
                return entity;
            }
        }

        public async Task<MealPlan> GetMealPlanAsync(Guid mealPlanResourceId) {
            var entity = await mealPlanRepository.GetAsync(mealPlanResourceId).ConfigureAwait(false);
            if (entity == null) {
                throw new MealPlanNotFoundException($"Meal plan with id {mealPlanResourceId} not found");
            }

            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
            if (entity.CreatedSubject?.SubjectId != currentSubjectId) {
                throw new MealPlanNotFoundException($"Meal plan with id {mealPlanResourceId} not found");
            }

            return entity;
        }

        public Task<PagedList<MealPlan>> SearchMealPlansAsync(MealPlanSearch search) {
            return mealPlanRepository.SearchAsync(search);
        }

        public async Task<MealPlan> UpdateMealPlanAsync(Guid resourceId, UpdateMealPlanDto dto) {
            var entity = await GetMealPlanAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("MealPlanResourceId", entity.MealPlanResourceId)) {
                entity.Update(dto.Name, dto.StartDate, dto.EndDate);

                if (dto.Entries != null) {
                    mealPlanRepository.RemoveEntries(entity);
                    await SetEntriesFromDtoAsync(entity, dto.Entries).ConfigureAwait(false);
                }

                logger.LogInformation("Updated existing meal plan");
                return entity;
            }
        }

        public async Task DeleteMealPlanAsync(Guid resourceId) {
            var entity = await GetMealPlanAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("MealPlanResourceId", entity.MealPlanResourceId)) {
                await mealPlanRepository.RemoveAsync(entity);
                logger.LogInformation("Deleted meal plan");
            }
        }

        public async Task<GroceryListDto> GenerateGroceryListAsync(Guid mealPlanResourceId) {
            var entity = await GetMealPlanAsync(mealPlanResourceId).ConfigureAwait(false);

            // Aggregate ingredients from non-leftover entries, scaling by servings
            var ingredientMap = new Dictionary<string, GroceryItemDto>();
            var sourceMap = new Dictionary<string, HashSet<string>>();

            foreach (var entry in entity.Entries.Where(e => !e.IsLeftover)) {
                var recipe = entry.Recipe;
                if (recipe?.Ingredients == null) {
                    continue;
                }

                var scaleFactor = entry.Servings.HasValue && recipe.Yield > 0
                    ? (decimal)entry.Servings.Value / recipe.Yield
                    : 1m;

                foreach (var ingredient in recipe.Ingredients) {
                    var key = $"{ingredient.Item?.ToLowerInvariant()}|{ingredient.Unit?.ToLowerInvariant()}";

                    if (ingredientMap.TryGetValue(key, out var existing)) {
                        if (existing.Quantity.HasValue && ingredient.Quantity.HasValue) {
                            existing.Quantity += ingredient.Quantity.Value * scaleFactor;
                        }
                    } else {
                        ingredientMap[key] = new GroceryItemDto {
                            Item = ingredient.Item,
                            Quantity = ingredient.Quantity.HasValue ? ingredient.Quantity.Value * scaleFactor : null,
                            Unit = ingredient.Unit
                        };
                    }

                    // Track recipe sources
                    if (!sourceMap.TryGetValue(key, out var sources)) {
                        sources = new HashSet<string>();
                        sourceMap[key] = sources;
                    }
                    if (!string.IsNullOrWhiteSpace(recipe.Title)) {
                        sources.Add(recipe.Title);
                    }
                }
            }

            // Attach sources to items
            foreach (var kvp in ingredientMap) {
                if (sourceMap.TryGetValue(kvp.Key, out var sources)) {
                    kvp.Value.Sources = sources.ToList();
                }
            }

            var rawItems = ingredientMap.Values.OrderBy(i => i.Item).ToList();

            // Use AI to consolidate similar ingredients and categorize
            try {
                var geminiItems = rawItems.Select(i => new GeminiGroceryItem {
                    Item = i.Item,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Sources = i.Sources
                }).ToList();

                var consolidated = await geminiClient.ConsolidateGroceryListAsync(geminiItems).ConfigureAwait(false);

                return new GroceryListDto {
                    Items = consolidated.Items.Select(i => new GroceryItemDto {
                        Item = i.Item,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Category = i.Category ?? "Other",
                        Sources = i.Sources ?? new List<string>()
                    }).ToList()
                };
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "Failed to consolidate grocery list with AI, returning raw list");
                return new GroceryListDto {
                    Items = rawItems.Select(i => {
                        i.Category = "Other";
                        return i;
                    }).ToList()
                };
            }
        }

        private async Task SetEntriesFromDtoAsync(MealPlan entity, List<UpdateMealPlanEntryDto> entryDtos) {
            var entries = new List<MealPlanEntry>();
            foreach (var entryDto in entryDtos) {
                var recipe = await recipeRepository.GetAsync(entryDto.RecipeResourceId).ConfigureAwait(false);
                if (recipe == null) {
                    throw new RecipeNotFoundException($"Recipe with id {entryDto.RecipeResourceId} not found");
                }
                entries.Add(new MealPlanEntry(entryDto.Date, (MealSlot)entryDto.MealSlot, recipe.RecipeId, entryDto.Servings, entryDto.IsLeftover));
            }
            entity.SetEntries(entries);
        }
    }
}
