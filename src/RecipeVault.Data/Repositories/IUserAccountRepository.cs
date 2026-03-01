using System;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IUserAccountRepository {
        Task<UserAccount> AddAsync(UserAccount userAccount);
        Task<UserAccount> GetAsync(Guid userAccountResourceId);
        Task<UserAccount> GetBySubjectIdAsync(Guid subjectId);
    }
}
