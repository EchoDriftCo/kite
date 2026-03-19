using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface ICookingLogRepository {
        Task<CookingLog> AddAsync(CookingLog cookingLog);
        Task<CookingLog> GetAsync(Guid cookingLogResourceId);
        Task<CookingLog> GetByIdAsync(int cookingLogId);
        Task<PagedList<CookingLog>> GetBySubjectIdAsync(Guid subjectId, int pageNumber, int pageSize);
        Task<List<CookingLog>> GetByRecipeIdAsync(int recipeId, Guid subjectId);
        Task<List<CookingLog>> GetBySubjectIdAndDateRangeAsync(Guid subjectId, DateTime startDate, DateTime endDate);
        Task RemoveAsync(CookingLog cookingLog);
    }
}
