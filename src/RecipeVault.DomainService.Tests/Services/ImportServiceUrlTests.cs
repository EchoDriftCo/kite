using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Shouldly;
using Xunit;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService;
using RecipeVault.DomainService.Tests.Base;
using RecipeVault.Integrations.Gemini;
using RecipeVault.Integrations.Gemini.Models;
using RecipeVault.Integrations.VideoDownload;

namespace RecipeVault.DomainService.Tests.Services {
    public class ImportServiceUrlTests : DomainServiceTestBase {
        private static Mock<IHttpClientFactory> CreateMockHttpClientFactory(string htmlContent, HttpStatusCode statusCode = HttpStatusCode.OK) {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = statusCode,
                    Content = new StringContent(htmlContent)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return mockFactory;
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithSchemaOrgRecipe_ParsesSuccessfully() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Chocolate Chip Cookies"",
                        ""recipeYield"": 24,
                        ""prepTime"": ""PT15M"",
                        ""cookTime"": ""PT12M"",
                        ""description"": ""Delicious homemade cookies"",
                        ""recipeIngredient"": [
                            ""2 cups all-purpose flour"",
                            ""1 cup butter, softened"",
                            ""3/4 cup sugar""
                        ],
                        ""recipeInstructions"": [
                            ""Preheat oven to 375°F"",
                            ""Mix dry ingredients"",
                            ""Bake for 12 minutes""
                        ],
                        ""image"": ""https://example.com/cookies.jpg"",
                        ""author"": ""Chef John"",
                        ""url"": ""https://example.com/recipe""
                    }
                    </script>
                </head>
                <body>Recipe content</body>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            Recipe capturedRecipe = null;
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("Chocolate Chip Cookies");
            result.Yield.ShouldBe(24);
            result.PrepTimeMinutes.ShouldBe(15);
            result.CookTimeMinutes.ShouldBe(12);
            result.Description.ShouldBe("Delicious homemade cookies");
            result.Source.ShouldBe("Chef John");
            result.OriginalImageUrl.ShouldBe("https://example.com/cookies.jpg");
            result.Ingredients.Count.ShouldBe(3);
            result.Ingredients[0].RawText.ShouldBe("2 cups all-purpose flour");
            result.Instructions.Count.ShouldBe(3);
            result.Instructions[0].Instruction.ShouldBe("Preheat oven to 375°F");
            result.IsPublic.ShouldBe(false);

            mockRecipeRepository.Verify(x => x.AddAsync(It.IsAny<Recipe>()), Times.Once);
            mockGeminiClient.Verify(x => x.ParseRecipeTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithSchemaOrgInArray_ParsesSuccessfully() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    [
                        {
                            ""@type"": ""WebPage"",
                            ""name"": ""Some Page""
                        },
                        {
                            ""@type"": ""Recipe"",
                            ""name"": ""Test Recipe"",
                            ""recipeYield"": 4,
                            ""prepTime"": ""PT10M"",
                            ""recipeIngredient"": [""flour""],
                            ""recipeInstructions"": [""Mix and bake""]
                        }
                    ]
                    </script>
                </head>
                <body>Recipe content</body>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.Title.ShouldBe("Test Recipe");
            result.PrepTimeMinutes.ShouldBe(10);
            mockGeminiClient.Verify(x => x.ParseRecipeTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithHowToStepInstructions_ParsesSuccessfully() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Test Recipe"",
                        ""recipeYield"": 4,
                        ""recipeIngredient"": [""flour""],
                        ""recipeInstructions"": [
                            {
                                ""@type"": ""HowToStep"",
                                ""text"": ""Step 1: Preheat oven""
                            },
                            {
                                ""@type"": ""HowToStep"",
                                ""text"": ""Step 2: Mix ingredients""
                            }
                        ]
                    }
                    </script>
                </head>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            Recipe capturedRecipe = null;
            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>(r => capturedRecipe = r)
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.Instructions.Count.ShouldBe(2);
            result.Instructions[0].Instruction.ShouldBe("Step 1: Preheat oven");
            result.Instructions[1].Instruction.ShouldBe("Step 2: Mix ingredients");
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithoutSchemaOrg_FallsBackToGemini() {
            // Arrange
            var html = @"
                <html>
                <body>
                    <h1>My Recipe</h1>
                    <p>Ingredients: flour, sugar, eggs</p>
                    <p>Mix everything and bake</p>
                </body>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            var geminiResponse = new GeminiParseResponse {
                Confidence = 0.95,
                Title = "My Recipe",
                Yield = 4,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 30,
                Ingredients = new List<GeminiIngredient> {
                    new GeminiIngredient { Quantity = 2, Unit = "cups", Item = "flour", RawText = "2 cups flour" },
                    new GeminiIngredient { Quantity = 1, Unit = "cup", Item = "sugar", RawText = "1 cup sugar" }
                },
                Instructions = new List<GeminiInstruction> {
                    new GeminiInstruction { StepNumber = 1, Instruction = "Mix ingredients", RawText = "Mix ingredients" },
                    new GeminiInstruction { StepNumber = 2, Instruction = "Bake at 350F", RawText = "Bake at 350F" }
                }
            };

            mockGeminiClient
                .Setup(x => x.ParseRecipeTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(geminiResponse);

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.Title.ShouldBe("My Recipe");
            result.Yield.ShouldBe(4);
            result.PrepTimeMinutes.ShouldBe(10);
            result.CookTimeMinutes.ShouldBe(30);
            result.Source.ShouldBe("https://example.com/recipe");
            result.Ingredients.Count.ShouldBe(2);
            result.Instructions.Count.ShouldBe(2);

            mockGeminiClient.Verify(x => x.ParseRecipeTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithImageAsObject_ParsesImageUrl() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Test Recipe"",
                        ""recipeYield"": 4,
                        ""recipeIngredient"": [""flour""],
                        ""recipeInstructions"": [""bake""],
                        ""image"": {
                            ""@type"": ""ImageObject"",
                            ""url"": ""https://example.com/image.jpg""
                        }
                    }
                    </script>
                </head>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.OriginalImageUrl.ShouldBe("https://example.com/image.jpg");
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithAuthorAsObject_ParsesAuthorName() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Test Recipe"",
                        ""recipeYield"": 4,
                        ""recipeIngredient"": [""flour""],
                        ""recipeInstructions"": [""bake""],
                        ""author"": {
                            ""@type"": ""Person"",
                            ""name"": ""Chef Bobby""
                        }
                    }
                    </script>
                </head>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.Source.ShouldBe("Chef Bobby");
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithRecipeYieldAsString_ParsesCorrectly() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Test Recipe"",
                        ""recipeYield"": ""6 servings"",
                        ""recipeIngredient"": [""flour""],
                        ""recipeInstructions"": [""bake""]
                    }
                    </script>
                </head>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.Yield.ShouldBe(6);
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithOnlyTotalTime_UsesTotalAsookTime() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Test Recipe"",
                        ""recipeYield"": 4,
                        ""totalTime"": ""PT1H"",
                        ""recipeIngredient"": [""flour""],
                        ""recipeInstructions"": [""bake""]
                    }
                    </script>
                </head>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.CookTimeMinutes.ShouldBe(60);
            result.PrepTimeMinutes.ShouldBeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-url")]
        [InlineData("ftp://example.com")]
        public async Task ImportFromUrlAsync_WithInvalidUrl_ThrowsArgumentException(string invalidUrl) {
            // Arrange
            var mockFactory = CreateMockHttpClientFactory("");
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(() => service.ImportFromUrlAsync(invalidUrl));
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithHttpError_ThrowsInvalidOperationException() {
            // Arrange
            var mockFactory = CreateMockHttpClientFactory("", HttpStatusCode.NotFound);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(() => service.ImportFromUrlAsync("https://example.com/recipe"));
        }

        [Fact]
        public async Task ImportFromUrlAsync_WithNoAuthor_UsesUrlAsSource() {
            // Arrange
            var html = @"
                <html>
                <head>
                    <script type=""application/ld+json"">
                    {
                        ""@type"": ""Recipe"",
                        ""name"": ""Test Recipe"",
                        ""recipeYield"": 4,
                        ""recipeIngredient"": [""flour""],
                        ""recipeInstructions"": [""bake""]
                    }
                    </script>
                </head>
                </html>";

            var mockFactory = CreateMockHttpClientFactory(html);
            var mockRecipeRepository = MockRepository.Create<IRecipeRepository>();
            var mockTagService = MockRepository.Create<ITagService>();
            var mockImageStorage = MockRepository.Create<IImageStorage>();
            var mockGeminiClient = MockRepository.Create<IGeminiClient>();
            var mockLogger = CreateMockLogger<ImportService>();

            mockRecipeRepository
                .Setup(x => x.AddAsync(It.IsAny<Recipe>()))
                .ReturnsAsync((Recipe r) => r);

            var service = new ImportService(
                mockRecipeRepository.Object,
                mockTagService.Object,
                mockImageStorage.Object,
                mockFactory.Object,
                mockGeminiClient.Object,
                Mock.Of<IVideoDownloadService>(),
                mockLogger.Object
            );

            // Act
            var result = await service.ImportFromUrlAsync("https://example.com/recipe");

            // Assert
            result.Source.ShouldBe("https://example.com/recipe");
        }
    }
}
