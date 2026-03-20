using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.DomainService.Tests.Base;
using RecipeVault.Integrations.VideoDownload;

namespace RecipeVault.DomainService.Tests.Services {
    public class ImportServiceStructuredTests : DomainServiceTestBase {
        private ImportService CreateService(
            Mock<IRecipeRepository> mockRecipeRepository,
            Mock<ITagService> mockTagService = null,
            Mock<IImageStorage> mockImageStorage = null) {
            mockTagService ??= MockRepository.Create<ITagService>();
            mockImageStorage ??= MockRepository.Create<IImageStorage>();
            var mockHttpClientFactory = MockRepository.Create<System.Net.Http.IHttpClientFactory>();
            var mockGeminiClient = MockRepository.Create<RecipeVault.Integrations.Gemini.IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();
            return new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockHttpClientFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object);
        }

        [Fact]
        public async Task ImportStructuredAsync_CreatesRecipeFromRawData() {
            // Arrange
            Recipe savedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => savedRecipe = r)
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository);

            var dto = new ImportStructuredRequestDto {
                Title = "Classic Margherita Pizza",
                Description = "A simple Italian pizza",
                Yield = 4,
                PrepTimeMinutes = 30,
                CookTimeMinutes = 15,
                Source = "https://example.com/margherita-pizza",
                OriginalImageUrl = "https://example.com/pizza.jpg",
                RawIngredients = new List<string> {
                    "2 cups all-purpose flour",
                    "1 cup warm water",
                    "8 oz fresh mozzarella"
                },
                RawInstructions = new List<string> {
                    "Combine flour and water. Knead for 10 minutes.",
                    "Let dough rise for 1 hour.",
                    "Top with mozzarella and bake."
                },
                Categories = null
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("Classic Margherita Pizza");
            result.Description.ShouldBe("A simple Italian pizza");
            result.Yield.ShouldBe(4);
            result.PrepTimeMinutes.ShouldBe(30);
            result.CookTimeMinutes.ShouldBe(15);
            result.Source.ShouldBe("https://example.com/margherita-pizza");
            result.OriginalImageUrl.ShouldBe("https://example.com/pizza.jpg");
            result.Ingredients.Count.ShouldBe(3);
            result.Instructions.Count.ShouldBe(3);

            mockRecipeRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task ImportStructuredAsync_ParsesIngredientStrings() {
            // Arrange
            Recipe savedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => savedRecipe = r)
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository);

            var dto = new ImportStructuredRequestDto {
                Title = "Test Recipe",
                RawIngredients = new List<string> {
                    "2 cups flour",
                    "1 tsp salt"
                },
                RawInstructions = new List<string>()
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            savedRecipe.Ingredients.Count.ShouldBe(2);
            savedRecipe.Ingredients[0].RawText.ShouldBe("2 cups flour");
            savedRecipe.Ingredients[0].SortOrder.ShouldBe(1);
            savedRecipe.Ingredients[1].RawText.ShouldBe("1 tsp salt");
            savedRecipe.Ingredients[1].SortOrder.ShouldBe(2);
        }

        [Fact]
        public async Task ImportStructuredAsync_SetsSourceUrl() {
            // Arrange
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository);

            var dto = new ImportStructuredRequestDto {
                Title = "Test Recipe",
                Source = "https://example.com/recipe",
                RawIngredients = new List<string>(),
                RawInstructions = new List<string>()
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            result.Source.ShouldBe("https://example.com/recipe");
        }

        [Fact]
        public async Task ImportStructuredAsync_HandlesMissingOptionalFields() {
            // Arrange
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository);

            var dto = new ImportStructuredRequestDto {
                Title = "Minimal Recipe",
                Description = null,
                Yield = null,
                PrepTimeMinutes = null,
                CookTimeMinutes = null,
                Source = null,
                OriginalImageUrl = null,
                RawIngredients = null,
                RawInstructions = null,
                Categories = null
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("Minimal Recipe");
            result.Yield.ShouldBe(4); // Default yield
            result.PrepTimeMinutes.ShouldBeNull();
            result.CookTimeMinutes.ShouldBeNull();
            result.Ingredients.Count.ShouldBe(0);
            result.Instructions.Count.ShouldBe(0);
        }

        [Fact]
        public async Task ImportStructuredAsync_AssignsTagsFromCategories() {
            // Arrange
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var mockTagService = MockRepository.Create<ITagService>();
            var tag1 = new Tag("Italian", TagCategory.Custom, false);
            var tag2 = new Tag("Dinner", TagCategory.Custom, false);
            mockTagService
                .Setup(x => x.GetOrCreateTagAsync("Italian", TagCategory.Custom))
                .ReturnsAsync(tag1)
                .Verifiable();
            mockTagService
                .Setup(x => x.GetOrCreateTagAsync("Dinner", TagCategory.Custom))
                .ReturnsAsync(tag2)
                .Verifiable();

            var service = CreateService(mockRecipeRepository, mockTagService);

            var dto = new ImportStructuredRequestDto {
                Title = "Test Recipe",
                RawIngredients = new List<string>(),
                RawInstructions = new List<string>(),
                Categories = new List<string> { "Italian", "Dinner" }
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            mockTagService.Verify(x => x.GetOrCreateTagAsync("Italian", TagCategory.Custom), Times.Once);
            mockTagService.Verify(x => x.GetOrCreateTagAsync("Dinner", TagCategory.Custom), Times.Once);
        }

        [Fact]
        public async Task ImportStructuredAsync_SetsInstructionsCorrectly() {
            // Arrange
            Recipe savedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => savedRecipe = r)
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository);

            var dto = new ImportStructuredRequestDto {
                Title = "Test Recipe",
                RawIngredients = new List<string>(),
                RawInstructions = new List<string> {
                    "Preheat oven to 350°F.",
                    "Mix all ingredients together.",
                    "Bake for 30 minutes."
                }
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            savedRecipe.Instructions.Count.ShouldBe(3);
            savedRecipe.Instructions[0].StepNumber.ShouldBe(1);
            savedRecipe.Instructions[0].Instruction.ShouldBe("Preheat oven to 350°F.");
            savedRecipe.Instructions[1].StepNumber.ShouldBe(2);
            savedRecipe.Instructions[2].StepNumber.ShouldBe(3);
        }

        [Fact]
        public async Task ImportStructuredAsync_RecipeIsPrivateByDefault() {
            // Arrange
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository);

            var dto = new ImportStructuredRequestDto {
                Title = "Test Recipe",
                RawIngredients = new List<string>(),
                RawInstructions = new List<string>()
            };

            // Act
            var result = await service.ImportStructuredAsync(dto);

            // Assert
            result.IsPublic.ShouldBeFalse();
        }
    }
}
