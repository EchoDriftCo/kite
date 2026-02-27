using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class CookingLogRepository : ICookingLogRepository {
        private readonly IRecipeVaultDbContext context;

        public CookingLogRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CookingLog> AddAsync(CookingLog cookingLog) {
            var entity = await context.CookingLogs.AddAsync(cookingLog);
            return entity.Entity;
        }

        public Task<CookingLog> GetAsync(Guid cookingLogResourceId) {
            return context.CookingLogs
                .Include(x => x.Recipe)
                .Include(x => x.Photos)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(cl => cl.CookingLogResourceId == cookingLogResourceId);
        }

        public Task<CookingLog> GetByIdAsync(int cookingLogId) {
            return context.CookingLogs
                .Include(x => x.Recipe)
                .Include(x => x.Photos)
                .Include(x => x.CreatedSubject)
                .FirstOrDefaultAsync(cl => cl.CookingLogId == cookingLogId);
        }

        public async Task<PagedList<CookingLog>> GetBySubjectIdAsync(Guid subjectId, int pageNumber, int pageSize) {
            var query = context.CookingLogs
                .Include(x => x.Recipe)
                .Include(x => x.Photos)
                .Include(x => x.CreatedSubject)
                .Where(cl => cl.CreatedSubject.SubjectId == subjectId)
                .OrderByDescending(cl => cl.CookedDate);

            var totalItems = await query.CountAsync().ConfigureAwait(false);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            return new PagedList<CookingLog> {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public Task<List<CookingLog>> GetByRecipeIdAsync(int recipeId, Guid subjectId) {
            return context.CookingLogs
                .Include(x => x.Photos)
                .Where(cl => cl.RecipeId == recipeId && cl.CreatedSubject.SubjectId == subjectId)
                .OrderByDescending(cl => cl.CookedDate)
                .ToListAsync();
        }

        public Task<List<CookingLog>> GetBySubjectIdAndDateRangeAsync(Guid subjectId, DateTime startDate, DateTime endDate) {
            return context.CookingLogs
                .Include(x => x.Recipe)
                .Include(x => x.Photos)
                .Where(cl => cl.CreatedSubject.SubjectId == subjectId 
                    && cl.CookedDate >= startDate 
                    && cl.CookedDate < endDate)
                .OrderBy(cl => cl.CookedDate)
                .ToListAsync();
        }

        public Task RemoveAsync(CookingLog cookingLog) {
            context.Remove(cookingLog);
            return Task.CompletedTask;
        }
    }
}
