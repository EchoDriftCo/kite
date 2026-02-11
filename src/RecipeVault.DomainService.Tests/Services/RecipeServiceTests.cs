using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Exceptions;
using RecipeVault.TestUtilities.Builders;
using RecipeVault.DomainService.Tests.Base;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService.Tests.Services {
    public class RecipeServiceTests : DomainServiceTestBase {
        private static readonly Guid TestSubjectId = Guid.NewGuid();

        private Mock<ISubjectPrincipal> CreateMockSubjectPrincipal(bool setupSubjectId = false) {
            var mock = MockRepository.Create<ISubjectPrincipal>();
            if (setupSubjectId) {
                mock.Setup(x => x.SubjectId).Returns(TestSubjectId.ToString());
            }
            return mock;
        }

        private static Recipe BuildRecipeWithOwner(RecipeBuilder builder = null) {
            builder ??= new RecipeBuilder();
            var recipe = builder.Build();
            recipe.CreatedSubject = new Subject(TestSubjectId, "Test User", "Test", "User", "test@example.com");
            return recipe;
        }

        [Fact]
        public async Task CreateRecipeAsync_WithValidDto_CreatesAndReturnsRecipe() {
            // Arrange
            var dto = new UpdateRecipeDtoBuilder().Build();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(dto.Title);
            result.Yield.ShouldBe(dto.Yield);
            result.Description.ShouldBe(dto.Description);
            result.RecipeResourceId.ShouldNotBe(Guid.Empty);

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithIngredients_AddsRecipeWithIngredients() {
            // Arrange
            var ingredients = new List<UpdateRecipeIngredientDto>
            {
                new UpdateRecipeIngredientDto { SortOrder = 1, Quantity = 2, Unit = "cups", Item = "flour", RawText = "2 cups flour" },
                new UpdateRecipeIngredientDto { SortOrder = 2, Quantity = 1, Unit = "cup", Item = "sugar", RawText = "1 cup sugar" }
            };

            var dto = new UpdateRecipeDtoBuilder()
                .WithIngredients(ingredients)
                .Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.Ingredients.Count.ShouldBe(2);
            result.Ingredients[0].Item.ShouldBe("flour");
            result.Ingredients[1].Item.ShouldBe("sugar");

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_WithInstructions_AddsRecipeWithInstructions() {
            // Arrange
            var instructions = new List<UpdateRecipeInstructionDto>
            {
                new UpdateRecipeInstructionDto { StepNumber = 1, Instruction = "Preheat oven", RawText = "Preheat oven" },
                new UpdateRecipeInstructionDto { StepNumber = 2, Instruction = "Mix ingredients", RawText = "Mix ingredients" }
            };

            var dto = new UpdateRecipeDtoBuilder()
                .WithInstructions(instructions)
                .Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe recipe) => recipe)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.CreateRecipeAsync(dto);

            // Assert
            result.Instructions.Count.ShouldBe(2);
            result.Instructions[0].Instruction.ShouldBe("Preheat oven");
            result.Instructions[1].Instruction.ShouldBe("Mix ingredients");

            mockRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithValidId_ReturnsRecipe() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.GetRecipeAsync(recipe.RecipeResourceId);

            // Assert
            result.ShouldBe(recipe);
            mockRepository.Verify(x => x.GetAsync(recipe.RecipeResourceId), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithInvalidId_ThrowsNotFoundException() {
            // Arrange
            var invalidId = Guid.NewGuid();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.GetAsync(invalidId))
                .ReturnsAsync((Recipe)null)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act & Assert
            var ex = await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.GetRecipeAsync(invalidId)
            );

            ex.Message.ShouldContain(invalidId.ToString());
            mockRepository.Verify(x => x.GetAsync(invalidId), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.GetRecipeAsync(recipe.RecipeResourceId)
            );
        }

        [Fact]
        public async Task UpdateRecipeAsync_WithValidData_UpdatesAndReturnsRecipe() {
            // Arrange
            var recipe = BuildRecipeWithOwner(new RecipeBuilder()
                .WithTitle("Original Title")
                .WithYield(4));

            var updateDto = new UpdateRecipeDtoBuilder()
                .WithTitle("Updated Title")
                .WithYield(8)
                .Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.UpdateRecipeAsync(recipe.RecipeResourceId, updateDto);

            // Assert
            result.Title.ShouldBe("Updated Title");
            result.Yield.ShouldBe(8);

            mockRepository.Verify(x => x.GetAsync(recipe.RecipeResourceId), Times.Once);
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithValidId_DeletesRecipe() {
            // Arrange
            var recipe = BuildRecipeWithOwner();
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe)
                .Verifiable();

            mockRepository
                .Setup(x => x.RemoveAsync(recipe))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            await service.DeleteRecipeAsync(recipe.RecipeResourceId);

            // Assert
            mockRepository.Verify(x => x.GetAsync(recipe.RecipeResourceId), Times.Once);
            mockRepository.Verify(x => x.RemoveAsync(recipe), Times.Once);
        }

        [Fact]
        public async Task GetRecipeAsync_WithPublicRecipeFromOtherUser_ReturnsRecipe() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(true).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.GetRecipeAsync(recipe.RecipeResourceId);

            // Assert
            result.ShouldBe(recipe);
            result.IsPublic.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateRecipeAsync_WithOtherUsersPublicRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(true).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var updateDto = new UpdateRecipeDtoBuilder().Build();

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.UpdateRecipeAsync(recipe.RecipeResourceId, updateDto)
            );
        }

        [Fact]
        public async Task DeleteRecipeAsync_WithOtherUsersPublicRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().WithIsPublic(true).Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.DeleteRecipeAsync(recipe.RecipeResourceId)
            );
        }

        [Fact]
        public async Task SetRecipeVisibilityAsync_WithOwnRecipe_SetsVisibility() {
            // Arrange
            var recipe = BuildRecipeWithOwner(new RecipeBuilder().WithIsPublic(false));
            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            await service.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true);

            // Assert
            recipe.IsPublic.ShouldBeTrue();
        }

        [Fact]
        public async Task SetRecipeVisibilityAsync_WithOtherUsersRecipe_ThrowsNotFoundException() {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var recipe = new RecipeBuilder().Build();
            recipe.CreatedSubject = new Subject(otherUserId, "Other User", "Other", "User", "other@example.com");

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal(setupSubjectId: true);

            mockRepository
                .Setup(x => x.GetAsync(recipe.RecipeResourceId))
                .ReturnsAsync(recipe);

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act & Assert
            await Should.ThrowAsync<RecipeNotFoundException>(
                () => service.SetRecipeVisibilityAsync(recipe.RecipeResourceId, true)
            );
        }

        [Fact]
        public async Task SearchRecipesAsync_WithValidSearch_ReturnsPagedResults() {
            // Arrange
            var recipes = new List<Recipe>
            {
                new RecipeBuilder().WithTitle("Recipe 1").Build(),
                new RecipeBuilder().WithTitle("Recipe 2").Build()
            };

            var pagedList = new PagedList<Recipe>
            {
                Items = recipes,
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 2
            };
            var search = new RecipeSearch { PageNumber = 1, PageSize = 10 };

            var mockRepository = MockRepository.Create<IRecipeRepository>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<RecipeService>();
            var mockSubjectPrincipal = CreateMockSubjectPrincipal();

            mockRepository
                .Setup(x => x.SearchAsync(search))
                .ReturnsAsync(pagedList)
                .Verifiable();

            var service = new RecipeService(mockRepository.Object, mockGeminiClient.Object, mockLogger.Object, mockSubjectPrincipal.Object);

            // Act
            var result = await service.SearchRecipesAsync(search);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.TotalItems.ShouldBe(2);

            mockRepository.Verify(x => x.SearchAsync(search), Times.Once);
        }
    }
}
