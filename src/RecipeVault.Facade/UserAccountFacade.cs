using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class UserAccountFacade : IUserAccountFacade {
        private readonly IUnitOfWork uow;
        private readonly IUserAccountService userAccountService;
        private readonly UserAccountMapper mapper;
        private readonly ILogger<UserAccountFacade> logger;
        private readonly ISubjectPrincipal subjectPrincipal;

        public UserAccountFacade(
            ILogger<UserAccountFacade> logger,
            IUnitOfWork uow,
            IUserAccountService userAccountService,
            UserAccountMapper mapper,
            ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.userAccountService = userAccountService;
            this.mapper = mapper;
            this.logger = logger;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<UserAccountDto> GetCurrentAccountAsync() {
            var account = await userAccountService.GetOrCreateAccountAsync(CurrentSubjectId).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(account);
        }

        public async Task<UserAccountDto> SetTierAsync(string tier) {
            if (!Enum.TryParse<AccountTier>(tier, true, out var accountTier)) {
                throw new ArgumentException($"Invalid tier value: {tier}. Valid values are: Free, Premium, Beta");
            }

            var account = await userAccountService.SetTierAsync(CurrentSubjectId, accountTier).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(account);
        }
    }
}
