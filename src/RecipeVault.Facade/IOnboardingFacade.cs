using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IOnboardingFacade {
        Task<OnboardingStatusDto> GetOnboardingStatusAsync();
        Task UpdateOnboardingProgressAsync(OnboardingProgressDto progress);
        Task<AddSampleRecipesResultDto> AddSampleRecipesAsync();
        Task<RemoveSampleRecipesResultDto> RemoveSampleRecipesAsync();
        Task CompleteOnboardingAsync();
        Task ResetOnboardingAsync();
    }
}
