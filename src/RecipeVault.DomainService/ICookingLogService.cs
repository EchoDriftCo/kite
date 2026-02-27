using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface ICookingLogService {
        Task<CookingLog> CreateCookingLogAsync(CreateCookingLogDto dto);
        Task<CookingLog> GetCookingLogAsync(Guid cookingLogResourceId);
        Task<PagedList<CookingLog>> GetCookingLogsAsync(int pageNumber, int pageSize);
        Task<CookingLog> UpdateCookingLogAsync(Guid cookingLogResourceId, UpdateCookingLogDto dto);
        Task DeleteCookingLogAsync(Guid cookingLogResourceId);
        Task<CookingLog> AddPhotosAsync(Guid cookingLogResourceId, List<CookingLogPhotoDto> photos);
        Task<CookingStatsDto> GetCookingStatsAsync();
        Task<List<CookingLog>> GetCalendarEntriesAsync(int year, int month);
        Task<List<CookingLog>> GetRecipeCookingHistoryAsync(Guid recipeResourceId);
        Task<RecipePersonalStatsDto> GetRecipePersonalStatsAsync(Guid recipeResourceId);
    }
}
