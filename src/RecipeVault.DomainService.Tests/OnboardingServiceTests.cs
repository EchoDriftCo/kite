using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using Xunit;

namespace RecipeVault.DomainService.Tests {
    public class OnboardingServiceTests {
        private readonly Mock<IUserAccountRepository> mockUserAccountRepository;
        private readonly Mock<IRecipeRepository> mockRecipeRepository;
        private readonly Mock<IRecipeService> mockRecipeService;
        private readonly Mock<ISubjectPrincipal> mockSubjectPrincipal;
        private readonly Mock<ILogger<OnboardingService>> mockLogger;
        private readonly OnboardingService service;

        private static readonly Guid SubjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid SystemSubjectId = Guid.Parse("d290f1ee-6c54-5f96-8b2f-9f14e72c8c39");

        public OnboardingServiceTests() {
            mockUserAccountRepository = new Mock<IUserAccountRepository>();
            mockRecipeRepository = new Mock<IRecipeRepository>();
            mockRecipeService = new Mock<IRecipeService>();
            mockSubjectPrincipal = new Mock<ISubjectPrincipal>();
            mockLogger = new Mock<ILogger<OnboardingService>>();

            mockSubjectPrincipal.Setup(x => x.SubjectId).Returns(SubjectId.ToString());

            service = new OnboardingService(
                mockUserAccountRepository.Object,
                mockRecipeRepository.Object,
                mockRecipeService.Object,
                mockSubjectPrincipal.Object,
                mockLogger.Object
            );
        }

        [Fact]
        public async Task GetOnboardingStatusAsync_NewUser_ReturnsFalse() {
            // Arrange
            var account = new UserAccount(SubjectId);
            mockUserAccountRepository.Setup(x => x.GetBySubjectIdAsync(SubjectId)).ReturnsAsync(account);
            mockRecipeRepository.Setup(x => x.GetCountByOwnerAsync(SubjectId)).ReturnsAsync(0);

            // Act
            var result = await service.GetOnboardingStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasCompletedOnboarding);
            Assert.Equal(0, result.RecipeCount);
            Assert.False(result.HasImportedRecipes);
            Assert.NotNull(result.Progress);
            Assert.False(result.Progress.DietaryProfileSet);
            Assert.False(result.Progress.SamplesAdded);
            Assert.False(result.Progress.TourCompleted);
        }

        [Fact]
        public async Task GetOnboardingStatusAsync_ExistingUser_ReturnsProgress() {
            // Arrange
            var account = new UserAccount(SubjectId);
            account.UpdateOnboardingProgress("{\"dietaryProfileSet\":true,\"samplesAdded\":false,\"tourCompleted\":false}");
            mockUserAccountRepository.Setup(x => x.GetBySubjectIdAsync(SubjectId)).ReturnsAsync(account);
            mockRecipeRepository.Setup(x => x.GetCountByOwnerAsync(SubjectId)).ReturnsAsync(6);

            // Act
            var result = await service.GetOnboardingStatusAsync();

            // Assert
            Assert.True(result.Progress.DietaryProfileSet);
            Assert.False(result.Progress.SamplesAdded);
            Assert.Equal(6, result.RecipeCount);
            Assert.True(result.HasImportedRecipes);
        }

        [Fact]
        public async Task UpdateOnboardingProgressAsync_SavesProgress() {
            // Arrange
            var account = new UserAccount(SubjectId);
            mockUserAccountRepository.Setup(x => x.GetBySubjectIdAsync(SubjectId)).ReturnsAsync(account);

            var progress = new OnboardingProgressDto { DietaryProfileSet = true };

            // Act
            await service.UpdateOnboardingProgressAsync(progress);

            // Assert
            Assert.NotNull(account.OnboardingProgressJson);
            Assert.Contains("dietaryProfileSet", account.OnboardingProgressJson);
        }

        [Fact]
        public async Task AddSampleRecipesAsync_ForksAllSamples_Returns6() {
            // Arrange
            var sampleRecipes = CreateSampleRecipes(6);
            mockRecipeRepository.Setup(x => x.GetSampleRecipesAsync(SystemSubjectId)).ReturnsAsync(sampleRecipes);
            mockRecipeRepository.Setup(x => x.GetForkSourceIdsAsync(SubjectId)).ReturnsAsync(new HashSet<int>());

            foreach (var sample in sampleRecipes) {
                var forked = new Recipe($"{sample.Title} (Copy)", sample.Yield, 15, 15, sample.Description, null, null);
                mockRecipeService.Setup(x => x.ForkRecipeAsync(sample.RecipeResourceId, null)).ReturnsAsync(forked);
            }

            // Act
            var result = await service.AddSampleRecipesAsync();

            // Assert
            Assert.Equal(6, result.RecipesAdded);
            Assert.Equal(6, result.Recipes.Count);
            mockRecipeService.Verify(x => x.ForkRecipeAsync(It.IsAny<Guid>(), null), Times.Exactly(6));
        }

        [Fact]
        public async Task AddSampleRecipesAsync_IdempotentCall_ReturnsZero() {
            // Arrange
            var sampleRecipes = CreateSampleRecipes(6);
            var existingForkIds = new HashSet<int>();
            foreach (var sample in sampleRecipes) {
                existingForkIds.Add(sample.RecipeId);
            }

            mockRecipeRepository.Setup(x => x.GetSampleRecipesAsync(SystemSubjectId)).ReturnsAsync(sampleRecipes);
            mockRecipeRepository.Setup(x => x.GetForkSourceIdsAsync(SubjectId)).ReturnsAsync(existingForkIds);

            // Act
            var result = await service.AddSampleRecipesAsync();

            // Assert
            Assert.Equal(0, result.RecipesAdded);
            Assert.Empty(result.Recipes);
            mockRecipeService.Verify(x => x.ForkRecipeAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddSampleRecipesAsync_MarksSamplesWithFlag() {
            // Arrange
            var sampleRecipes = CreateSampleRecipes(1);
            mockRecipeRepository.Setup(x => x.GetSampleRecipesAsync(SystemSubjectId)).ReturnsAsync(sampleRecipes);
            mockRecipeRepository.Setup(x => x.GetForkSourceIdsAsync(SubjectId)).ReturnsAsync(new HashSet<int>());

            var forked = new Recipe("Test Recipe (Copy)", 4, 15, 15, "Test", null, null);
            mockRecipeService.Setup(x => x.ForkRecipeAsync(sampleRecipes[0].RecipeResourceId, null)).ReturnsAsync(forked);

            // Act
            await service.AddSampleRecipesAsync();

            // Assert
            Assert.True(forked.IsSampleRecipe);
        }

        [Fact]
        public async Task RemoveSampleRecipesAsync_DeletesOnlySamples() {
            // Arrange
            var sampleRecipes = CreateSampleRecipes(3);
            foreach (var r in sampleRecipes) {
                r.MarkAsSample();
            }
            mockRecipeRepository.Setup(x => x.GetSampleRecipesByOwnerAsync(SubjectId)).ReturnsAsync(sampleRecipes);
            mockRecipeService.Setup(x => x.DeleteRecipeAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            // Act
            var result = await service.RemoveSampleRecipesAsync();

            // Assert
            Assert.Equal(3, result.RecipesRemoved);
            mockRecipeService.Verify(x => x.DeleteRecipeAsync(It.IsAny<Guid>()), Times.Exactly(3));
        }

        [Fact]
        public async Task CompleteOnboardingAsync_SetsFlag_SetsDate() {
            // Arrange
            var account = new UserAccount(SubjectId);
            mockUserAccountRepository.Setup(x => x.GetBySubjectIdAsync(SubjectId)).ReturnsAsync(account);

            // Act
            await service.CompleteOnboardingAsync();

            // Assert
            Assert.True(account.HasCompletedOnboarding);
            Assert.NotNull(account.OnboardingCompletedDate);
        }

        [Fact]
        public async Task ResetOnboardingAsync_ClearsFlag_ClearsDate_ClearsProgress() {
            // Arrange
            var account = new UserAccount(SubjectId);
            account.CompleteOnboarding();
            account.UpdateOnboardingProgress("{\"dietaryProfileSet\":true}");
            mockUserAccountRepository.Setup(x => x.GetBySubjectIdAsync(SubjectId)).ReturnsAsync(account);

            // Act
            await service.ResetOnboardingAsync();

            // Assert
            Assert.False(account.HasCompletedOnboarding);
            Assert.Null(account.OnboardingCompletedDate);
            Assert.Null(account.OnboardingProgressJson);
        }

        [Fact]
        public void SystemAccountId_NotGuidEmpty() {
            Assert.NotEqual(Guid.Empty, SystemSubjectId);
        }

        [Fact]
        public void SystemAccountId_IsValidUUID() {
            Assert.True(Guid.TryParse("d290f1ee-6c54-5f96-8b2f-9f14e72c8c39", out _));
        }

        [Fact]
        public void SampleRecipes_Count_Is6() {
            var recipes = Data.SampleRecipeSeedData.GetSampleRecipes();
            Assert.Equal(6, recipes.Count);
        }

        [Fact]
        public void AllSampleRecipes_HaveShowcases() {
            var recipes = Data.SampleRecipeSeedData.GetSampleRecipes();
            foreach (var recipe in recipes) {
                Assert.False(string.IsNullOrEmpty(recipe.Showcases));
            }
        }

        [Fact]
        public void SpicyBibimbap_IsForkedFrom_KoreanBibimbap() {
            var recipes = Data.SampleRecipeSeedData.GetSampleRecipes();
            var spicyBibimbap = recipes.Find(r => r.Title == "Spicy Bibimbap");
            Assert.NotNull(spicyBibimbap);
            Assert.True(spicyBibimbap.IsForkedFromBibimbap);
        }

        [Fact]
        public void ThaiBasilChicken_HasIngredients() {
            var recipes = Data.SampleRecipeSeedData.GetSampleRecipes();
            var thaiBasil = recipes.Find(r => r.Title.Contains("Thai Basil"));
            Assert.NotNull(thaiBasil);
            Assert.True(thaiBasil.Ingredients.Count >= 10);
        }

        [Fact]
        public void Shakshuka_HasTimerInstructions() {
            var recipes = Data.SampleRecipeSeedData.GetSampleRecipes();
            var shakshuka = recipes.Find(r => r.Title.Contains("Shakshuka"));
            Assert.NotNull(shakshuka);
            var timerSteps = shakshuka.Instructions.FindAll(i => i.Instruction.Contains("[TIMER:"));
            Assert.Equal(3, timerSteps.Count);
        }

        private static List<Recipe> CreateSampleRecipes(int count) {
            var recipes = new List<Recipe>();
            for (int i = 0; i < count; i++) {
                var recipe = new Recipe($"Sample Recipe {i + 1}", 4, 15, 15, $"Description {i + 1}", null, null, true);
                recipes.Add(recipe);
            }
            return recipes;
        }
    }
}
