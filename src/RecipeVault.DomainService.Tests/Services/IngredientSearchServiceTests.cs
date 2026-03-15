using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using Shouldly;
using Xunit;

namespace RecipeVault.DomainService.Tests.Services {
    public class IngredientSearchServiceTests : IDisposable {
        private readonly Mock<IRecipeRepository> recipeRepositoryMock;
        private readonly Mock<IUserPantryRepository> userPantryRepositoryMock;
        private readonly Mock<ISubjectPrincipal> subjectPrincipalMock;
        private readonly Mock<ILogger<IngredientSearchService>> loggerMock;
        private readonly DbContext dbContext;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly Mock<IConfigurationSection> configSectionMock;
        private readonly IngredientSearchService service;
        private readonly Guid currentSubjectId = Guid.NewGuid();

        public IngredientSearchServiceTests() {
            recipeRepositoryMock = new Mock<IRecipeRepository>();
            userPantryRepositoryMock = new Mock<IUserPantryRepository>();
            subjectPrincipalMock = new Mock<ISubjectPrincipal>();
            loggerMock = new Mock<ILogger<IngredientSearchService>>();
            configurationMock = new Mock<IConfiguration>();
            configSectionMock = new Mock<IConfigurationSection>();

            subjectPrincipalMock.Setup(x => x.SubjectId).Returns(currentSubjectId.ToString());
            configurationMock.Setup(x => x.GetSection("IngredientSearch:DefaultPantryStaples"))
                .Returns(configSectionMock.Object);

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            dbContext = new DbContext(options);

            service = new IngredientSearchService(
                recipeRepositoryMock.Object,
                userPantryRepositoryMock.Object,
                subjectPrincipalMock.Object,
                loggerMock.Object,
                dbContext,
                configurationMock.Object
            );
        }

        public void Dispose() {
            dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task SearchByIngredientsAsync_WhenFirstAccess_ShouldSeedPantry() {
            // Arrange
            userPantryRepositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(0);
            userPantryRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<List<UserPantryItem>>()))
                .Returns(Task.CompletedTask);
            userPantryRepositoryMock.Setup(x => x.GetStaplesAsync(currentSubjectId))
                .ReturnsAsync(new List<string> { "salt", "pepper" });

            var request = new IngredientSearchRequestDto {
                Ingredients = new List<string> { "chicken" },
                IncludePantryStaples = true
            };

            // Act - will throw because InMemory provider doesn't support raw SQL,
            // but pantry seeding happens before the DB scoring call
            try {
                await service.SearchByIngredientsAsync(request);
            } catch (Exception) {
                // Expected - InMemory connection cannot be cast to NpgsqlConnection
            }

            // Assert - seeding was attempted with 10 default items
            userPantryRepositoryMock.Verify(x => x.CountAsync(currentSubjectId), Times.Once);
            userPantryRepositoryMock.Verify(x => x.AddRangeAsync(It.Is<List<UserPantryItem>>(items =>
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

            // Act
            try {
                await service.SearchByIngredientsAsync(request);
            } catch (Exception) {
                // Expected - InMemory connection cannot be cast to NpgsqlConnection
            }

            // Assert - seeding was NOT attempted
            userPantryRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<UserPantryItem>>()), Times.Never);
        }

        [Fact]
        public async Task SearchByIngredientsAsync_WhenPantryStaplesDisabled_ShouldNotFetchStaples() {
            // Arrange
            userPantryRepositoryMock.Setup(x => x.CountAsync(currentSubjectId)).ReturnsAsync(5);

            var request = new IngredientSearchRequestDto {
                Ingredients = new List<string> { "chicken" },
                IncludePantryStaples = false
            };

            // Act
            try {
                await service.SearchByIngredientsAsync(request);
            } catch (Exception) {
                // Expected - InMemory connection cannot be cast to NpgsqlConnection
            }

            // Assert - staples were NOT fetched
            userPantryRepositoryMock.Verify(x => x.GetStaplesAsync(currentSubjectId), Times.Never);
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullDependencies() {
            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                null, userPantryRepositoryMock.Object, subjectPrincipalMock.Object,
                loggerMock.Object, dbContext, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, null, subjectPrincipalMock.Object,
                loggerMock.Object, dbContext, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, userPantryRepositoryMock.Object, null,
                loggerMock.Object, dbContext, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, userPantryRepositoryMock.Object,
                subjectPrincipalMock.Object, null, dbContext, configurationMock.Object));

            Should.Throw<ArgumentNullException>(() => new IngredientSearchService(
                recipeRepositoryMock.Object, userPantryRepositoryMock.Object,
                subjectPrincipalMock.Object, loggerMock.Object, null, configurationMock.Object));
        }
    }
}
