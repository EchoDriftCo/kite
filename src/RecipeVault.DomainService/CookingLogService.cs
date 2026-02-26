using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class CookingLogService : ICookingLogService {
        private readonly ILogger<CookingLogService> logger;
        private readonly ICookingLogRepository cookingLogRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public CookingLogService(
            ICookingLogRepository cookingLogRepository,
            IRecipeRepository recipeRepository,
            ILogger<CookingLogService> logger,
            ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.cookingLogRepository = cookingLogRepository;
            this.recipeRepository = recipeRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<CookingLog> CreateCookingLogAsync(CreateCookingLogDto dto) {
            var recipe = await recipeRepository.GetAsync(dto.RecipeResourceId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {dto.RecipeResourceId} not found");
            }

            // Verify the user owns this recipe or it's public
            if (recipe.CreatedSubject?.SubjectId != CurrentSubjectId && !recipe.IsPublic) {
                throw new RecipeNotFoundException($"Recipe with id {dto.RecipeResourceId} not found");
            }

            var entity = new CookingLog(
                recipe.RecipeId,
                dto.CookedDate,
                dto.ScaleFactor,
                dto.ServingsMade,
                dto.Notes,
                dto.Rating
            );

            await cookingLogRepository.AddAsync(entity);
            logger.LogInformation("Created new cooking log for recipe {RecipeId}", recipe.RecipeId);
            
            return entity;
        }

        public async Task<CookingLog> GetCookingLogAsync(Guid cookingLogResourceId) {
            var entity = await cookingLogRepository.GetAsync(cookingLogResourceId).ConfigureAwait(false);
            if (entity == null || entity.CreatedSubject?.SubjectId != CurrentSubjectId) {
                throw new CookingLogNotFoundException($"Cooking log with id {cookingLogResourceId} not found");
            }
            return entity;
        }

        public Task<PagedList<CookingLog>> GetCookingLogsAsync(int pageNumber, int pageSize) {
            return cookingLogRepository.GetBySubjectIdAsync(CurrentSubjectId, pageNumber, pageSize);
        }

        public async Task<CookingLog> UpdateCookingLogAsync(Guid cookingLogResourceId, UpdateCookingLogDto dto) {
            var entity = await GetCookingLogAsync(cookingLogResourceId).ConfigureAwait(false);
            
            entity.Update(
                entity.RecipeId,
                dto.CookedDate,
                dto.ScaleFactor,
                dto.ServingsMade,
                dto.Notes,
                dto.Rating
            );

            logger.LogInformation("Updated cooking log {CookingLogResourceId}", cookingLogResourceId);
            return entity;
        }

        public async Task DeleteCookingLogAsync(Guid cookingLogResourceId) {
            var entity = await GetCookingLogAsync(cookingLogResourceId).ConfigureAwait(false);
            await cookingLogRepository.RemoveAsync(entity);
            logger.LogInformation("Deleted cooking log {CookingLogResourceId}", cookingLogResourceId);
        }

        public async Task<CookingLog> AddPhotosAsync(Guid cookingLogResourceId, List<CookingLogPhotoDto> photos) {
            var entity = await GetCookingLogAsync(cookingLogResourceId).ConfigureAwait(false);
            
            var newPhotos = photos.Select((p, index) => 
                new CookingLogPhoto(entity.CookingLogId, p.ImageUrl, p.Caption, p.SortOrder ?? index)
            ).ToList();
            
            foreach (var photo in newPhotos) {
                entity.AddPhoto(photo);
            }

            logger.LogInformation("Added {PhotoCount} photos to cooking log {CookingLogResourceId}", photos.Count, cookingLogResourceId);
            return entity;
        }

        public async Task<CookingStatsDto> GetCookingStatsAsync() {
            var currentYear = DateTime.UtcNow.Year;
            var startOfYear = new DateTime(currentYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = startOfYear.AddYears(1);

            var allLogs = await cookingLogRepository.GetBySubjectIdAndDateRangeAsync(
                CurrentSubjectId, 
                new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                DateTime.UtcNow.AddDays(1)
            ).ConfigureAwait(false);

            var logsThisYear = allLogs.Where(cl => cl.CookedDate >= startOfYear && cl.CookedDate < endOfYear).ToList();

            var totalCooks = allLogs.Count;
            var uniqueRecipes = allLogs.Select(cl => cl.RecipeId).Distinct().Count();
            var cooksThisYear = logsThisYear.Count;

            // Calculate current streak
            var currentStreak = CalculateCurrentStreak(allLogs);

            // Calculate longest streak
            var longestStreak = CalculateLongestStreak(allLogs);

            // Most cooked recipes
            var mostCooked = allLogs
                .GroupBy(cl => new { cl.RecipeId, cl.Recipe.Title })
                .Select(g => new MostCookedRecipeDto {
                    RecipeId = g.Key.RecipeId,
                    RecipeResourceId = g.First().Recipe.RecipeResourceId,
                    RecipeTitle = g.Key.Title,
                    CookCount = g.Count(),
                    AverageRating = g.Where(x => x.Rating.HasValue).Any() 
                        ? g.Where(x => x.Rating.HasValue).Average(x => x.Rating.Value) 
                        : null,
                    LastCookedDate = g.Max(x => x.CookedDate)
                })
                .OrderByDescending(x => x.CookCount)
                .Take(5)
                .ToList();

            return new CookingStatsDto {
                TotalCooks = totalCooks,
                UniqueRecipes = uniqueRecipes,
                CooksThisYear = cooksThisYear,
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                MostCookedRecipes = mostCooked
            };
        }

        private static int CalculateCurrentStreak(List<CookingLog> logs) {
            if (logs.Count == 0) {
                return 0;
            }

            var sortedDates = logs
                .Select(cl => cl.CookedDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Streak must start today or yesterday
            if (sortedDates.First() != today && sortedDates.First() != yesterday) {
                return 0;
            }

            var streak = 0;
            var currentDate = sortedDates.First();

            foreach (var date in sortedDates) {
                if (date == currentDate || date == currentDate.AddDays(-1)) {
                    streak++;
                    currentDate = date;
                } else {
                    break;
                }
            }

            return streak;
        }

        private static int CalculateLongestStreak(List<CookingLog> logs) {
            if (logs.Count == 0) {
                return 0;
            }

            var sortedDates = logs
                .Select(cl => cl.CookedDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var maxStreak = 1;
            var currentStreak = 1;

            for (int i = 1; i < sortedDates.Count; i++) {
                if (sortedDates[i] == sortedDates[i - 1].AddDays(1)) {
                    currentStreak++;
                    maxStreak = Math.Max(maxStreak, currentStreak);
                } else {
                    currentStreak = 1;
                }
            }

            return maxStreak;
        }

        public async Task<List<CookingLog>> GetCalendarEntriesAsync(int year, int month) {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            return await cookingLogRepository.GetBySubjectIdAndDateRangeAsync(CurrentSubjectId, startDate, endDate)
                .ConfigureAwait(false);
        }

        public async Task<List<CookingLog>> GetRecipeCookingHistoryAsync(Guid recipeResourceId) {
            var recipe = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            return await cookingLogRepository.GetByRecipeIdAsync(recipe.RecipeId, CurrentSubjectId)
                .ConfigureAwait(false);
        }

        public async Task<RecipePersonalStatsDto> GetRecipePersonalStatsAsync(Guid recipeResourceId) {
            var recipe = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (recipe == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            var logs = await cookingLogRepository.GetByRecipeIdAsync(recipe.RecipeId, CurrentSubjectId)
                .ConfigureAwait(false);

            if (logs.Count == 0) {
                return null;
            }

            var ratedLogs = logs.Where(l => l.Rating.HasValue).ToList();

            return new RecipePersonalStatsDto {
                CookCount = logs.Count,
                AverageRating = ratedLogs.Count > 0 ? ratedLogs.Average(l => l.Rating.Value) : null,
                LastCookedDate = logs.Max(l => l.CookedDate)
            };
        }
    }
}
