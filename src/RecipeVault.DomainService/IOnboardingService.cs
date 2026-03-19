using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IOnboardingService {
        Task<OnboardingStatusDto> GetOnboardingStatusAsync();
        Task UpdateOnboardingProgressAsync(OnboardingProgressDto progress);
        Task<AddSampleRecipesResultDto> AddSampleRecipesAsync();
        Task<RemoveSampleRecipesResultDto> RemoveSampleRecipesAsync();
        Task CompleteOnboardingAsync();
        Task ResetOnboardingAsync();
    }
}
