using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class ApiTokenRepository : IApiTokenRepository {
        private readonly IRecipeVaultDbContext context;

        public ApiTokenRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ApiToken> AddAsync(ApiToken apiToken) {
            var entity = await context.ApiTokens.AddAsync(apiToken);
            return entity.Entity;
        }

        public Task<ApiToken> GetAsync(Guid apiTokenResourceId) {
            return context.ApiTokens
                .FirstOrDefaultAsync(t => t.ApiTokenResourceId == apiTokenResourceId);
        }

        public Task<ApiToken> GetByHashAsync(string tokenHash) {
            return context.ApiTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public Task<List<ApiToken>> GetBySubjectIdAsync(Guid subjectId) {
            return context.ApiTokens
                .Where(t => t.SubjectId == subjectId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }
    }
}
