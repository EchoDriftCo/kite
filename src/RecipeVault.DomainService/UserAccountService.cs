using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;

namespace RecipeVault.DomainService {
    public class UserAccountService : IUserAccountService {
        private readonly ILogger<UserAccountService> logger;
        private readonly IUserAccountRepository userAccountRepository;

        public UserAccountService(
            IUserAccountRepository userAccountRepository,
            ILogger<UserAccountService> logger) {
            this.logger = logger;
            this.userAccountRepository = userAccountRepository;
        }

        public async Task<UserAccount> GetOrCreateAccountAsync(Guid subjectId) {
            var account = await userAccountRepository.GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
            if (account != null) {
                return account;
            }

            account = new UserAccount(subjectId);
            await userAccountRepository.AddAsync(account).ConfigureAwait(false);
            logger.LogInformation("Created new UserAccount for subject {SubjectId} with Free tier", subjectId);
            return account;
        }

        public async Task<UserAccount> SetTierAsync(Guid subjectId, AccountTier tier) {
            var account = await GetOrCreateAccountAsync(subjectId).ConfigureAwait(false);
            account.SetTier(tier);
            logger.LogInformation("Set account tier to {Tier} for subject {SubjectId}", tier, subjectId);
            return account;
        }

        public async Task<AccountTier> GetTierAsync(Guid subjectId) {
            var account = await userAccountRepository.GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
            return account?.AccountTier ?? AccountTier.Free;
        }

        public async Task<bool> IsPremiumOrBetaAsync(Guid subjectId) {
            var tier = await GetTierAsync(subjectId).ConfigureAwait(false);
            return tier == AccountTier.Premium || tier == AccountTier.Beta;
        }

        public async Task<UserAccount> MarkBetaCodeRedeemedAsync(Guid subjectId) {
            var account = await GetOrCreateAccountAsync(subjectId).ConfigureAwait(false);
            account.SetBetaCodeRedeemed();
            logger.LogInformation("Marked beta code as redeemed for subject {SubjectId}", subjectId);
            return account;
        }

        public async Task<bool> HasRedeemedBetaCodeAsync(Guid subjectId) {
            var account = await userAccountRepository.GetBySubjectIdAsync(subjectId).ConfigureAwait(false);
            return account?.BetaCodeRedeemedDate.HasValue ?? false;
        }
    }
}
