using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface ICookingLogFacade {
        Task<CookingLogDto> CreateCookingLogAsync(CreateCookingLogDto dto);
        Task<CookingLogDto> GetCookingLogAsync(Guid cookingLogResourceId);
        Task<PagedList<CookingLogDto>> GetCookingLogsAsync(int pageNumber, int pageSize);
        Task<CookingLogDto> UpdateCookingLogAsync(Guid cookingLogResourceId, UpdateCookingLogDto dto);
        Task DeleteCookingLogAsync(Guid cookingLogResourceId);
        Task<CookingLogDto> AddPhotosAsync(Guid cookingLogResourceId, List<CookingLogPhotoDto> photos);
        Task<CookingStatsDto> GetCookingStatsAsync();
        Task<List<CookingLogDto>> GetCalendarEntriesAsync(int year, int month);
        Task<List<CookingLogDto>> GetRecipeCookingHistoryAsync(Guid recipeResourceId);
        Task<RecipePersonalStatsDto> GetRecipePersonalStatsAsync(Guid recipeResourceId);
    }
}
