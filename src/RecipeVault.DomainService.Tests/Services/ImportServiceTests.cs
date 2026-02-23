using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService;
using RecipeVault.DomainService.Models;
using RecipeVault.DomainService.Tests.Base;

namespace RecipeVault.DomainService.Tests.Services {
    public class ImportServiceTests : DomainServiceTestBase {
        private ImportService CreateService(
            Mock<IRecipeRepository> mockRecipeRepository,
            Mock<ITagService> mockTagService,
            Mock<IImageStorage> mockImageStorage) {
            var mockHttpClientFactory = MockRepository.Create<System.Net.Http.IHttpClientFactory>();
            var mockGeminiClient = MockRepository.Create<RecipeVault.Integrations.Gemini.IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();
            return new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockHttpClientFactory.Object,
                mockGeminiClient.Object,
                mockLogger.Object);
        }

        private static MemoryStream CreatePaprikaStream(List<PaprikaRecipe> recipes) {
            var json = JsonSerializer.Serialize(recipes);
            var memoryStream = new MemoryStream();
            
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true)) {
                var bytes = Encoding.UTF8.GetBytes(json);
                gzipStream.Write(bytes, 0, bytes.Length);
            }
            
            memoryStream.Position = 0;
            return memoryStream;
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithValidRecipe_ImportsSuccessfully() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "2 cups flour\n1 cup sugar",
                    Directions = "Mix ingredients\nBake at 350°F",
                    Servings = "4",
                    PrepTime = "15 minutes",
                    CookTime = "30 minutes",
                    Source = "Test Source",
                    Notes = "Test notes"
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r)
                .Verifiable();

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            var result = await service.ImportFromPaprikaAsync(stream);

            // Assert
            result.ShouldNotBeNull();
            result.TotalRecipes.ShouldBe(1);
            result.SuccessCount.ShouldBe(1);
            result.FailureCount.ShouldBe(0);
            result.ImportedRecipes.Count.ShouldBe(1);
            result.ImportedRecipes[0].Title.ShouldBe("Test Recipe");
            result.Errors.Count.ShouldBe(0);

            mockRecipeRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithMultipleRecipes_ImportsAll() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe { Name = "Recipe 1", Ingredients = "flour", Directions = "mix", Servings = "4" },
                new PaprikaRecipe { Name = "Recipe 2", Ingredients = "sugar", Directions = "bake", Servings = "2" },
                new PaprikaRecipe { Name = "Recipe 3", Ingredients = "eggs", Directions = "whisk", Servings = "6" }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            var result = await service.ImportFromPaprikaAsync(stream);

            // Assert
            result.TotalRecipes.ShouldBe(3);
            result.SuccessCount.ShouldBe(3);
            result.FailureCount.ShouldBe(0);
            result.ImportedRecipes.Count.ShouldBe(3);

            mockRecipeRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithIngredients_ParsesCorrectly() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "2 cups flour\n1 cup sugar\n3 eggs",
                    Directions = "Mix and bake",
                    Servings = "4"
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.ShouldNotBeNull();
            capturedRecipe.Ingredients.Count.ShouldBe(3);
            capturedRecipe.Ingredients[0].RawText.ShouldBe("2 cups flour");
            capturedRecipe.Ingredients[1].RawText.ShouldBe("1 cup sugar");
            capturedRecipe.Ingredients[2].RawText.ShouldBe("3 eggs");
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithInstructions_ParsesCorrectly() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "Step 1: Preheat oven\nStep 2: Mix ingredients\nStep 3: Bake",
                    Servings = "4"
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.ShouldNotBeNull();
            capturedRecipe.Instructions.Count.ShouldBe(3);
            capturedRecipe.Instructions[0].Instruction.ShouldBe("Step 1: Preheat oven");
            capturedRecipe.Instructions[1].Instruction.ShouldBe("Step 2: Mix ingredients");
            capturedRecipe.Instructions[2].Instruction.ShouldBe("Step 3: Bake");
        }

        [Theory]
        [InlineData("4", 4)]
        [InlineData("6 servings", 6)]
        [InlineData("Serves 8", 8)]
        [InlineData("2-4 servings", 2)]
        [InlineData("", 4)] // Default
        [InlineData(null, 4)] // Default
        public async Task ImportFromPaprikaAsync_ParsesServings_Correctly(string servingsInput, int expectedYield) {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = servingsInput
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.Yield.ShouldBe(expectedYield);
        }

        [Theory]
        [InlineData("30 minutes", 30)]
        [InlineData("1 hour", 60)]
        [InlineData("1 hour 30 minutes", 90)]
        [InlineData("2 hrs 15 min", 135)]
        [InlineData("45", 45)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public async Task ImportFromPaprikaAsync_ParsesTime_Correctly(string timeInput, int? expectedMinutes) {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = "4",
                    PrepTime = timeInput
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.PrepTimeMinutes.ShouldBe(expectedMinutes);
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithCategories_CreatesTagsCorrectly() {
            // Arrange
            var categories = new[] { "Dessert", "Quick" };
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = "4",
                    Categories = categories
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            var dessertTag = new Tag("Dessert", TagCategory.Custom, false);
            var quickTag = new Tag("Quick", TagCategory.Custom, false);

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            mockTagService
                .Setup(x => x.GetOrCreateTagAsync("Dessert", TagCategory.Custom))
                .ReturnsAsync(dessertTag);

            mockTagService
                .Setup(x => x.GetOrCreateTagAsync("Quick", TagCategory.Custom))
                .ReturnsAsync(quickTag);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            mockTagService.Verify(x => x.GetOrCreateTagAsync("Dessert", TagCategory.Custom), Times.Once);
            mockTagService.Verify(x => x.GetOrCreateTagAsync("Quick", TagCategory.Custom), Times.Once);
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithPhotoData_UploadsImage() {
            // Arrange
            var base64Image = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = "4",
                    PhotoData = base64Image
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            mockImageStorage
                .Setup(x => x.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://example.com/image.jpg");

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            mockImageStorage.Verify(x => x.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            capturedRecipe.SourceImageUrl.ShouldBe("https://example.com/image.jpg");
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithFailedRecipe_ContinuesWithOthers() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe { Name = "Good Recipe 1", Ingredients = "flour", Directions = "mix", Servings = "4" },
                new PaprikaRecipe { Name = null, Ingredients = "sugar", Directions = "bake", Servings = "2" }, // This will fail
                new PaprikaRecipe { Name = "Good Recipe 2", Ingredients = "eggs", Directions = "whisk", Servings = "6" }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            var result = await service.ImportFromPaprikaAsync(stream);

            // Assert
            result.TotalRecipes.ShouldBe(3);
            result.SuccessCount.ShouldBe(2);
            result.FailureCount.ShouldBe(1);
            result.ImportedRecipes.Count.ShouldBe(2);
            result.Errors.Count.ShouldBe(1);

            mockRecipeRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithSource_SetsSourceCorrectly() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = "4",
                    Source = "My Cookbook",
                    SourceUrl = "https://example.com"
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.Source.ShouldBe("My Cookbook");
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_WithOnlySourceUrl_UsesUrlAsSource() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = "4",
                    Source = "",
                    SourceUrl = "https://example.com/recipe"
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.Source.ShouldBe("https://example.com/recipe");
        }

        [Fact]
        public async Task ImportFromPaprikaAsync_SetsRecipesAsPrivate() {
            // Arrange
            var paprikaRecipes = new List<PaprikaRecipe> {
                new PaprikaRecipe {
                    Name = "Test Recipe",
                    Ingredients = "flour",
                    Directions = "mix",
                    Servings = "4"
                }
            };

            var stream = CreatePaprikaStream(paprikaRecipes);

            Recipe capturedRecipe = null;
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = CreateService(mockRecipeRepository, mockTagService, mockImageStorage);

            // Act
            await service.ImportFromPaprikaAsync(stream);

            // Assert
            capturedRecipe.IsPublic.ShouldBe(false);
        }
    }
}
