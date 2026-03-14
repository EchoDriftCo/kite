using System.Linq;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class OnboardingModelMapper {
        public OnboardingStatusModel Map(OnboardingStatusDto dto) {
            if (dto == null) {
                return null;
            }

            return new OnboardingStatusModel {
                HasCompletedOnboarding = dto.HasCompletedOnboarding,
                RecipeCount = dto.RecipeCount,
                HasDietaryProfile = dto.HasDietaryProfile,
                HasImportedRecipes = dto.HasImportedRecipes,
                Progress = dto.Progress != null ? new OnboardingProgressModel {
                    DietaryProfileSet = dto.Progress.DietaryProfileSet,
                    SamplesAdded = dto.Progress.SamplesAdded,
                    TourCompleted = dto.Progress.TourCompleted
                } : new OnboardingProgressModel()
            };
        }

        public AddSampleRecipesResultModel MapResult(AddSampleRecipesResultDto dto) {
            if (dto == null) {
                return null;
            }

            return new AddSampleRecipesResultModel {
                RecipesAdded = dto.RecipesAdded,
                Recipes = dto.Recipes?.Select(r => new AddedRecipeModel {
                    RecipeResourceId = r.RecipeResourceId,
                    Title = r.Title,
                    Showcases = r.Showcases
                }).ToList()
            };
        }

        public RemoveSampleRecipesResultModel MapRemoveResult(RemoveSampleRecipesResultDto dto) {
            if (dto == null) {
                return null;
            }

            return new RemoveSampleRecipesResultModel {
                RecipesRemoved = dto.RecipesRemoved
            };
        }

        public OnboardingProgressDto MapToDto(UpdateOnboardingProgressModel model) {
            if (model == null) {
                return null;
            }

            return new OnboardingProgressDto {
                DietaryProfileSet = model.DietaryProfileSet,
                SamplesAdded = model.SamplesAdded,
                TourCompleted = model.TourCompleted
            };
        }
    }
}
