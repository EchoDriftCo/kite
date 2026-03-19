using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public class OnboardingService : IOnboardingService {
        private readonly ILogger<OnboardingService> logger;
        private readonly IUserAccountRepository userAccountRepository;
        private readonly IRecipeRepository recipeRepository;
        private readonly IRecipeService recipeService;
        private readonly IDietaryProfileService dietaryProfileService;
        private readonly ISubjectPrincipal subjectPrincipal;

        private static readonly Guid SystemSubjectId = Guid.Parse("d290f1ee-6c54-5f96-8b2f-9f14e72c8c39");

        private static readonly JsonSerializerOptions JsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public OnboardingService(
            IUserAccountRepository userAccountRepository,
            IRecipeRepository recipeRepository,
            IRecipeService recipeService,
            IDietaryProfileService dietaryProfileService,
            ISubjectPrincipal subjectPrincipal,
            ILogger<OnboardingService> logger) {
            this.userAccountRepository = userAccountRepository;
            this.recipeRepository = recipeRepository;
            this.recipeService = recipeService;
            this.dietaryProfileService = dietaryProfileService;
            this.subjectPrincipal = subjectPrincipal;
            this.logger = logger;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<OnboardingStatusDto> GetOnboardingStatusAsync() {
            var account = await userAccountRepository.GetBySubjectIdAsync(CurrentSubjectId).ConfigureAwait(false);
            var recipeCount = await recipeRepository.GetCountByOwnerAsync(CurrentSubjectId).ConfigureAwait(false);
            var dietaryProfiles = await dietaryProfileService.GetProfilesBySubjectAsync().ConfigureAwait(false);

            var progress = new OnboardingProgressDto();
            if (account != null && !string.IsNullOrEmpty(account.OnboardingProgressJson)) {
                try {
                    progress = JsonSerializer.Deserialize<OnboardingProgressDto>(account.OnboardingProgressJson, JsonOptions);
                } catch (JsonException ex) {
                    logger.LogWarning(ex, "Failed to deserialize onboarding progress for user {SubjectId}", CurrentSubjectId);
                }
            }

            return new OnboardingStatusDto {
                HasCompletedOnboarding = account?.HasCompletedOnboarding ?? false,
                RecipeCount = recipeCount,
                HasDietaryProfile = dietaryProfiles.Count > 0,
                HasImportedRecipes = recipeCount > 0,
                Progress = progress
            };
        }

        public async Task UpdateOnboardingProgressAsync(OnboardingProgressDto progress) {
            var account = await userAccountRepository.GetBySubjectIdAsync(CurrentSubjectId).ConfigureAwait(false);
            if (account == null) {
                logger.LogWarning("Cannot update onboarding progress: account not found for {SubjectId}", CurrentSubjectId);
                return;
            }

            // Merge with existing progress
            var existing = new OnboardingProgressDto();
            if (!string.IsNullOrEmpty(account.OnboardingProgressJson)) {
                try {
                    existing = JsonSerializer.Deserialize<OnboardingProgressDto>(account.OnboardingProgressJson, JsonOptions);
                } catch (JsonException) {
                    // Use defaults
                }
            }

            // Only update fields that are explicitly set to true (don't allow unsetting)
            if (progress.DietaryProfileSet) {
                existing.DietaryProfileSet = true;
            }
            if (progress.SamplesAdded) {
                existing.SamplesAdded = true;
            }
            if (progress.TourCompleted) {
                existing.TourCompleted = true;
            }

            var json = JsonSerializer.Serialize(existing, JsonOptions);
            account.UpdateOnboardingProgress(json);

            logger.LogInformation("Updated onboarding progress for user {SubjectId}: {Progress}", CurrentSubjectId, json);
        }

        public async Task<AddSampleRecipesResultDto> AddSampleRecipesAsync() {
            var sampleRecipes = await recipeRepository.GetSampleRecipesAsync(SystemSubjectId).ConfigureAwait(false);
            var existingForks = await recipeRepository.GetForkSourceIdsAsync(CurrentSubjectId).ConfigureAwait(false);

            var results = new List<AddedRecipeDto>();
            foreach (var sample in sampleRecipes) {
                if (existingForks.Contains(sample.RecipeId)) {
                    continue;
                }

                var forked = await recipeService.ForkRecipeAsync(sample.RecipeResourceId).ConfigureAwait(false);
                forked.MarkAsSample();

                results.Add(new AddedRecipeDto {
                    RecipeResourceId = forked.RecipeResourceId,
                    Title = forked.Title,
                    Showcases = sample.ShowcaseFeature ?? "general"
                });
            }

            logger.LogInformation("Added {Count} sample recipes for user {SubjectId}", results.Count, CurrentSubjectId);

            return new AddSampleRecipesResultDto {
                RecipesAdded = results.Count,
                Recipes = results
            };
        }

        public async Task<RemoveSampleRecipesResultDto> RemoveSampleRecipesAsync() {
            var sampleRecipes = await recipeRepository.GetSampleRecipesByOwnerAsync(CurrentSubjectId).ConfigureAwait(false);

            foreach (var recipe in sampleRecipes) {
                await recipeService.DeleteRecipeAsync(recipe.RecipeResourceId).ConfigureAwait(false);
            }

            logger.LogInformation("Removed {Count} sample recipes for user {SubjectId}", sampleRecipes.Count, CurrentSubjectId);

            return new RemoveSampleRecipesResultDto {
                RecipesRemoved = sampleRecipes.Count
            };
        }

        public async Task CompleteOnboardingAsync() {
            var account = await userAccountRepository.GetBySubjectIdAsync(CurrentSubjectId).ConfigureAwait(false);
            if (account == null) {
                logger.LogWarning("Cannot complete onboarding: account not found for {SubjectId}", CurrentSubjectId);
                return;
            }

            account.CompleteOnboarding();
            logger.LogInformation("Completed onboarding for user {SubjectId}", CurrentSubjectId);
        }

        public async Task ResetOnboardingAsync() {
            var account = await userAccountRepository.GetBySubjectIdAsync(CurrentSubjectId).ConfigureAwait(false);
            if (account == null) {
                logger.LogWarning("Cannot reset onboarding: account not found for {SubjectId}", CurrentSubjectId);
                return;
            }

            account.ResetOnboarding();
            logger.LogInformation("Reset onboarding for user {SubjectId}", CurrentSubjectId);
        }

    }
}
