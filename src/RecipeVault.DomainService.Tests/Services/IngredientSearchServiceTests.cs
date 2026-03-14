using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Dto.Input;
using Shouldly;
using Xunit;

namespace RecipeVault.DomainService.Tests.Services {
    public class IngredientSearchServiceTests {
        private readonly Mock<IRecipeRepository> recipeRepositoryMock;
        private readonly Mock<IUserPantryRepository> userPantryRepositoryMock;
        private readonly Mock<ISubjectPrincipal> subjectPrincipalMock;
        private readonly Mock<ILogger<IngredientSearchService>> loggerMock;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly IngredientSearchService service;
        private readonly Guid currentSubjectId = Guid.NewGuid();

        public IngredientSearchServiceTests() {
            recipeRepositoryMock = new Mock<IRecipeRepository>();
            userPantryRepositoryMock = new Mock<IUserPantryRepository>();
            subjectPrincipalMock = new Mock<ISubjectPrincipal>();
            loggerMock = new Mock<ILogger<IngredientSearchService>>();
            configurationMock = new Mock<IConfiguration>();

            subjectPrincipalMock.Setup(x => x.SubjectId).Returns(currentSubjectId.ToString());
            configurationMock.Setup(x => x["Database:ConnectionString"]).Returns("Host=localhost;Database=test");

            service = new IngredientSearchService(
                recipeRepositoryMock.Object,
                userPantryRepositoryMock.Object,
                subjectPrincipalMock.Object,
                loggerMock.Object,
                configurationMock.Object
            );
        }

        [Fact]
        public async Task SearchByIngredientsAsync_WhenFirstAccess_ShouldSeedPantry() {
            // Arrange
            userPantryRepositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(0);
            userPantryRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<List<Domain.Entities.UserPantryItem>>()))
                .Returns(Task.CompletedTask);
            userPantryRepositoryMock.Setup(x => x.GetStaplesAsync(currentSubjectId))
                .ReturnsAsync(new List<string> { "salt", "pepper" });

            var request = new IngredientSearchRequestDto {
                Ingredients = new List<string> { "chicken" },
                IncludePantryStaples = true
            };

            // Act & Assert - will throw because no real DB connection,
            // but we can verify the seeding was attempted
            try {
                await service.SearchByIngredientsAsync(request);
            } catch (Exception) {
                // Expected - no real database connection for the scoring function
            }

            // Verify seeding was attempted
            userPantryRepositoryMock.Verify(x => x.CountAsync(currentSubjectId), Times.Once);
            userPantryRepositoryMock.Verify(x => x.AddRangeAsync(It.Is<List<Domain.Entities.UserPantryItem>>(items =>
                items.Count == 10)), Times.Once);
        }

        [Fact]
        public async Task SearchByIngredientsAsync_WhenExistingUser_ShouldNotSeedPantry() {
            // Arrange
            userPantryRepositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(5);
            userPantryRepositoryMock.Setup(x => x.GetStaplesAsync(currentSubjectId))
                .ReturnsAsync(new List<string> { "salt", "pepper" });

            var request = new IngredientSearchRequestDto {
                Ingredients = new List<string> { "chicken" },
                IncludePantryStaples = true
            };

            // Act & Assert
            try {
                await service.SearchByIngredientsAsync(request);
            } catch (Exception) {
                // Expected - no real database connection
            }

            // Verify seeding was NOT attempted
            userPantryRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Domain.Entities.UserPantryItem>>()), Times.Never);
        }

        [Fact]
        public async Task SearchByIngredientsAsync_WhenPantryStaplesDisabled_ShouldNotFetchStaples() {
            // Arrange
            userPantryRepositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(5);

            var request = new IngredientSearchRequestDto {
                Ingredients = new List<string> { "chicken" },
                IncludePantryStaples = false
            };

            // Act & Assert
            try {
                await service.SearchByIngredientsAsync(request);
            } catch (Exception) {
                // Expected - no real database connection
            }

            // Verify staples were NOT fetched
            userPantryRepositoryMock.Verify(x => x.GetStaplesAsync(currentSubjectId), Times.Never);
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullDependencies() {
            // Assert
            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                null, userPantryRepositoryMock.Object, subjectPrincipalMock.Object,
                loggerMock.Object, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, null, subjectPrincipalMock.Object,
                loggerMock.Object, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, userPantryRepositoryMock.Object, null,
                loggerMock.Object, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, userPantryRepositoryMock.Object,
                subjectPrincipalMock.Object, null, configurationMock.Object));
        }
    }
}
