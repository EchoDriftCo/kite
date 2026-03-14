using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public class OnboardingFacade : IOnboardingFacade {
        private readonly IUnitOfWork uow;
        private readonly IOnboardingService onboardingService;
        private readonly ILogger<OnboardingFacade> logger;

        public OnboardingFacade(
            ILogger<OnboardingFacade> logger,
            IUnitOfWork uow,
            IOnboardingService onboardingService) {
            this.uow = uow;
            this.onboardingService = onboardingService;
            this.logger = logger;
        }

        public async Task<OnboardingStatusDto> GetOnboardingStatusAsync() {
            return await onboardingService.GetOnboardingStatusAsync().ConfigureAwait(false);
        }

        public async Task UpdateOnboardingProgressAsync(OnboardingProgressDto progress) {
            await onboardingService.UpdateOnboardingProgressAsync(progress).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<AddSampleRecipesResultDto> AddSampleRecipesAsync() {
            var result = await onboardingService.AddSampleRecipesAsync().ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return result;
        }

        public async Task<RemoveSampleRecipesResultDto> RemoveSampleRecipesAsync() {
            var result = await onboardingService.RemoveSampleRecipesAsync().ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return result;
        }

        public async Task CompleteOnboardingAsync() {
            await onboardingService.CompleteOnboardingAsync().ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ResetOnboardingAsync() {
            await onboardingService.ResetOnboardingAsync().ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
