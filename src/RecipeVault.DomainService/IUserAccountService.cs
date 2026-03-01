using System;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.DomainService {
    public interface IUserAccountService {
        Task<UserAccount> GetOrCreateAccountAsync(Guid subjectId);
        Task<UserAccount> SetTierAsync(Guid subjectId, AccountTier tier);
        Task<AccountTier> GetTierAsync(Guid subjectId);
        Task<bool> IsPremiumOrBetaAsync(Guid subjectId);
    }
}
