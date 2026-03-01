using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class UserAccountRepository : IUserAccountRepository {
        private readonly IRecipeVaultDbContext context;

        public UserAccountRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UserAccount> AddAsync(UserAccount userAccount) {
            var entity = await context.UserAccounts.AddAsync(userAccount);
            return entity.Entity;
        }

        public Task<UserAccount> GetAsync(Guid userAccountResourceId) {
            return context.UserAccounts
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(x => x.UserAccountResourceId == userAccountResourceId);
        }

        public Task<UserAccount> GetBySubjectIdAsync(Guid subjectId) {
            return context.UserAccounts
                .FirstOrDefaultAsync(x => x.SubjectId == subjectId);
        }
    }
}
