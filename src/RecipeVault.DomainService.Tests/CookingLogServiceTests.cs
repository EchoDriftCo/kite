using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using Xunit;
using Cortside.Common.Security;
using Cortside.AspNetCore.Common.Paging;

namespace RecipeVault.DomainService.Tests {
    public class CookingLogServiceTests {
        private readonly Mock<ICookingLogRepository> mockCookingLogRepository;
        private readonly Mock<IRecipeRepository> mockRecipeRepository;
        private readonly Mock<ILogger<CookingLogService>> mockLogger;
        private readonly Mock<ISubjectPrincipal> mockSubjectPrincipal;
        private readonly CookingLogService service;

        private static readonly Guid SubjectId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid RecipeResourceId1 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public CookingLogServiceTests() {
            mockCookingLogRepository = new Mock<ICookingLogRepository>();
            mockRecipeRepository = new Mock<IRecipeRepository>();
            mockLogger = new Mock<ILogger<CookingLogService>>();
            mockSubjectPrincipal = new Mock<ISubjectPrincipal>();

            mockSubjectPrincipal.Setup(x => x.SubjectId).Returns(SubjectId1.ToString());

            service = new CookingLogService(
                mockCookingLogRepository.Object,
                mockRecipeRepository.Object,
                mockLogger.Object,
                mockSubjectPrincipal.Object
            );
        }

        [Fact]
        public async Task CreateCookingLogAsync_WithValidRecipe_ShouldCreateCookingLog() {
            // Arrange
            var recipe = new Recipe("Test Recipe", 4, 30, 45, "Test description", null, null);
            recipe.SetVisibility(true); // Make the recipe public so anyone can log a cook
            
            // Use reflection to set RecipeId since it's database-generated
            var recipeIdProperty = typeof(Recipe).GetProperty("RecipeId");
            recipeIdProperty.SetValue(recipe, 123);
            
            var dto = new CreateCookingLogDto {
                RecipeResourceId = RecipeResourceId1,
                CookedDate = DateTime.UtcNow,
                ScaleFactor = 1.0m,
                ServingsMade = 4,
                Notes = "Turned out great!",
                Rating = 5
            };

            mockRecipeRepository
                .Setup(x => x.GetAsync(RecipeResourceId1))
                .ReturnsAsync(recipe);

            mockCookingLogRepository
                .Setup(x => x.AddAsync(It.IsAny<CookingLog>()))
                .ReturnsAsync((CookingLog cl) => cl);

            // Act
            var result = await service.CreateCookingLogAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.RecipeId);
            Assert.Equal(dto.CookedDate, result.CookedDate);
            Assert.Equal(dto.Rating, result.Rating);
            Assert.Equal(dto.Notes, result.Notes);
            mockCookingLogRepository.Verify(x => x.AddAsync(It.IsAny<CookingLog>()), Times.Once);
        }

        [Fact]
        public async Task CreateCookingLogAsync_WithNonExistentRecipe_ShouldThrowException() {
            // Arrange
            var dto = new CreateCookingLogDto {
                RecipeResourceId = RecipeResourceId1,
                CookedDate = DateTime.UtcNow,
                Rating = 5
            };

            mockRecipeRepository
                .Setup(x => x.GetAsync(RecipeResourceId1))
                .ReturnsAsync((Recipe)null);

            // Act & Assert
            await Assert.ThrowsAsync<RecipeNotFoundException>(() => service.CreateCookingLogAsync(dto));
        }

        [Fact]
        public async Task GetCookingLogsAsync_ShouldReturnPagedResults() {
            // Arrange
            var pagedLogs = new PagedList<CookingLog> {
                Items = new List<CookingLog> {
                    new CookingLog(1, DateTime.UtcNow.AddDays(-5), 1.0m, 4, "Good", 5),
                    new CookingLog(1, DateTime.UtcNow.AddDays(-3), 1.0m, 4, "Great", 5)
                },
                PageNumber = 1,
                PageSize = 20,
                TotalItems = 2
            };

            mockCookingLogRepository
                .Setup(x => x.GetBySubjectIdAsync(SubjectId1, 1, 20))
                .ReturnsAsync(pagedLogs);

            // Act
            var result = await service.GetCookingLogsAsync(1, 20);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetRecipePersonalStatsAsync_ShouldIncludeMostRecentNonEmptyNote() {
            // Arrange
            var recipe = new Recipe("Test Recipe", 4, 30, 45, "Test description", null, null);
            var recipeIdProperty = typeof(Recipe).GetProperty("RecipeId");
            recipeIdProperty.SetValue(recipe, 123);

            mockRecipeRepository
                .Setup(x => x.GetAsync(RecipeResourceId1))
                .ReturnsAsync(recipe);

            var logs = new List<CookingLog> {
                new CookingLog(123, DateTime.UtcNow.AddDays(-5), 1.0m, 4, "", 4),
                new CookingLog(123, DateTime.UtcNow.AddDays(-3), 1.0m, 4, "Use less salt", 5),
                new CookingLog(123, DateTime.UtcNow.AddDays(-1), 1.0m, 4, "Add extra garlic", 5)
            };

            mockCookingLogRepository
                .Setup(x => x.GetByRecipeIdAsync(123, SubjectId1))
                .ReturnsAsync(logs);

            // Act
            var result = await service.GetRecipePersonalStatsAsync(RecipeResourceId1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.CookCount);
            Assert.Equal("Add extra garlic", result.LastNote);
        }
    }
}
