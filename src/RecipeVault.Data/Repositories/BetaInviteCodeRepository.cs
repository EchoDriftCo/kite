using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class BetaInviteCodeRepository : IBetaInviteCodeRepository {
        private readonly IRecipeVaultDbContext dbContext;

        public BetaInviteCodeRepository(IRecipeVaultDbContext dbContext) {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<BetaInviteCode> AddAsync(BetaInviteCode betaInviteCode) {
            var entity = await dbContext.BetaInviteCodes.AddAsync(betaInviteCode).ConfigureAwait(false);
            return entity.Entity;
        }

        public Task<BetaInviteCode> GetAsync(Guid betaInviteCodeResourceId) {
            return dbContext.BetaInviteCodes
                .Include(c => c.Redemptions)
                .FirstOrDefaultAsync(c => c.BetaInviteCodeResourceId == betaInviteCodeResourceId);
        }

        public Task<BetaInviteCode> GetByCodeAsync(string code) {
            return dbContext.BetaInviteCodes
                .Include(c => c.Redemptions)
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<PagedList<BetaInviteCode>> SearchAsync(BetaInviteCodeSearch search) {
            var query = search.Build(dbContext.BetaInviteCodes
                .Include(c => c.Redemptions));

            var result = new PagedList<BetaInviteCode> {
                PageNumber = search.PageNumber,
                PageSize = search.PageSize,
                TotalItems = await query.CountAsync().ConfigureAwait(false),
                Items = []
            };

            query = query.ToSortedQuery(search.Sort);
            result.Items = await query.ToPagedQuery(search.PageNumber, search.PageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }
    }
}
