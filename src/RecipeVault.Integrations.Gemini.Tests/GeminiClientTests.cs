using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeVault.Integrations.Gemini.Exceptions;
using RecipeVault.Integrations.Gemini.Tests.Mocks;
using Shouldly;
using Xunit;

namespace RecipeVault.Integrations.Gemini.Tests {
    public class GeminiClientTests : IDisposable {
        private readonly Mock<IConfiguration> configurationMock;
        private readonly Mock<ILogger<GeminiClient>> loggerMock;
        private readonly GeminiMockServer mockServer;
        private readonly HttpClient httpClient;

        public GeminiClientTests() {
            configurationMock = new Mock<IConfiguration>();
            loggerMock = new Mock<ILogger<GeminiClient>>();

            // Setup logger mock to allow any calls
            loggerMock
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            loggerMock
                .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
                .Returns(new NullDisposable());

            // Create mock server
            mockServer = new GeminiMockServer();

            // Create HTTP client pointing to mock server
            httpClient = new HttpClient {
                BaseAddress = new Uri(mockServer.Url)
            };
        }

        public void Dispose() {
            httpClient?.Dispose();
            mockServer?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithValidImage_ReturnsSuccessfulResponse() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";
            var model = "gemini-1.5-flash";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns(model);

            mockServer.StubParseRecipeSuccess(
                title: "Chocolate Cake",
                yield: 8,
                prepTimeMinutes: 15,
                cookTimeMinutes: 30,
                ingredients: new List<MockIngredient> {
                    new() { Quantity = 2, Unit = "cup", Item = "flour", Preparation = null, RawText = "2 cups flour" }
                },
                instructions: new List<MockInstruction> {
                    new() { StepNumber = 1, Instruction = "Preheat oven to 350°F", RawText = "Preheat oven to 350°F" }
                });

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.ParseRecipeAsync(imageBase64, mimeType);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("Chocolate Cake");
            result.Yield.ShouldBe(8);
            result.PrepTimeMinutes.ShouldBe(15);
            result.CookTimeMinutes.ShouldBe(30);
            result.Ingredients.ShouldNotBeEmpty();
            result.Ingredients[0].Item.ShouldBe("flour");
            result.Instructions.ShouldNotBeEmpty();
            result.Instructions[0].Instruction.ShouldBe("Preheat oven to 350°F");
        }

        [Fact]
        public async Task ParseRecipeAsync_WithNoRecipeDetected_ThrowsGeminiApiException() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.StubNoRecipeDetected();

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, mimeType));

            ex.StatusCode.ShouldBe(422);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithServiceUnavailable_ThrowsGeminiApiException() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.StubServiceUnavailable();

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, mimeType));

            ex.StatusCode.ShouldBe(503);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithValidImage_CalculatesConfidenceScore() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";
            var model = "gemini-1.5-flash";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns(model);

            mockServer.StubParseRecipeSuccess();

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.ParseRecipeAsync(imageBase64, mimeType);

            // Assert
            result.Confidence.ShouldBeGreaterThan(0);
            result.Confidence.ShouldBeLessThanOrEqualTo(1);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithMissingFields_ReturnsWarnings() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";
            var model = "gemini-1.5-flash";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns(model);

            mockServer.StubParseRecipeWithMissingFields(title: "Chocolate Cake");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.ParseRecipeAsync(imageBase64, mimeType);

            // Assert
            result.Warnings.ShouldNotBeEmpty();
            result.Confidence.ShouldBeLessThan(1);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithUnauthorized_ThrowsGeminiApiException() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "invalid-api-key";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.StubUnauthorized();

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, mimeType));

            ex.StatusCode.ShouldBe(401);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithNullImageBase64_ThrowsArgumentException() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                geminiClient.ParseRecipeAsync(null, "image/png"));
        }

        [Fact]
        public async Task ParseRecipeAsync_WithNullMimeType_ThrowsArgumentException() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, null));
        }

        [Fact]
        public async Task ParseRecipeAsync_WithNoApiKey_ThrowsGeminiApiException() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";

            // Clear environment variable to ensure test isolation
            var originalEnvVar = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);

            try {
                configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns((string)null);
                configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

                var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

                // Act & Assert
                var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                    geminiClient.ParseRecipeAsync(imageBase64, mimeType));

                ex.Message.ShouldContain("API key");
            } finally {
                // Restore original environment variable
                Environment.SetEnvironmentVariable("GEMINI_API_KEY", originalEnvVar);
            }
        }

        [Fact]
        public async Task ConsolidateGroceryListAsync_WithNullList_ReturnsEmptyResponse() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.ConsolidateGroceryListAsync(null);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task ConsolidateGroceryListAsync_WithEmptyList_ReturnsEmptyResponse() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.ConsolidateGroceryListAsync(new List<GeminiGroceryItem>());

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task ConsolidateGroceryListAsync_WithValidItems_ReturnsConsolidatedList() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.Reset().StubConsolidateGrocerySuccess(new List<MockConsolidatedItem> {
                new() { Item = "all-purpose flour", Quantity = 5m, Unit = "cup", Category = "Pantry", Sources = new List<string> { "Banana Bread", "Pizza Dough" } },
                new() { Item = "sugar", Quantity = 0.5m, Unit = "cup", Category = "Pantry", Sources = new List<string> { "Banana Bread" } },
                new() { Item = "yeast", Quantity = 1m, Unit = "tsp", Category = "Pantry", Sources = new List<string> { "Pizza Dough" } }
            });

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            var items = new List<GeminiGroceryItem> {
                new() { Item = "flour", Quantity = 2m, Unit = "cup", Sources = new List<string> { "Banana Bread" } },
                new() { Item = "flour", Quantity = 3m, Unit = "cup", Sources = new List<string> { "Pizza Dough" } },
                new() { Item = "sugar", Quantity = 0.5m, Unit = "cup", Sources = new List<string> { "Banana Bread" } },
                new() { Item = "yeast", Quantity = 1m, Unit = "tsp", Sources = new List<string> { "Pizza Dough" } }
            };

            // Act
            var result = await geminiClient.ConsolidateGroceryListAsync(items);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(3);

            var flour = result.Items.First(i => i.Item == "all-purpose flour");
            flour.Quantity.ShouldBe(5m);
            flour.Unit.ShouldBe("cup");
            flour.Category.ShouldBe("Pantry");
            flour.Sources.ShouldContain("Banana Bread");
            flour.Sources.ShouldContain("Pizza Dough");

            var sugar = result.Items.First(i => i.Item == "sugar");
            sugar.Category.ShouldBe("Pantry");
            sugar.Sources.ShouldContain("Banana Bread");
        }

        [Fact]
        public async Task ConsolidateGroceryListAsync_WithItemsHavingSources_IncludesSourcesInPrompt() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.Reset().StubConsolidateGrocerySuccess(new List<MockConsolidatedItem> {
                new() { Item = "milk", Quantity = 2m, Unit = "cup", Category = "Dairy", Sources = new List<string> { "Smoothie", "Pancakes" } }
            });

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            var items = new List<GeminiGroceryItem> {
                new() { Item = "milk", Quantity = 2m, Unit = "cup", Sources = new List<string> { "Smoothie", "Pancakes" } }
            };

            // Act
            var result = await geminiClient.ConsolidateGroceryListAsync(items);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items[0].Sources.Count.ShouldBe(2);
            result.Items[0].Sources.ShouldContain("Smoothie");
            result.Items[0].Sources.ShouldContain("Pancakes");
            result.Items[0].Category.ShouldBe("Dairy");
        }

        [Fact]
        public async Task ConsolidateGroceryListAsync_WithNullCategoryInResponse_DefaultsToOther() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.Reset().StubConsolidateGrocerySuccess(new List<MockConsolidatedItem> {
                new() { Item = "obscure spice", Quantity = 1m, Unit = "tsp", Category = null, Sources = null }
            });

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            var items = new List<GeminiGroceryItem> {
                new() { Item = "obscure spice", Quantity = 1m, Unit = "tsp" }
            };

            // Act
            var result = await geminiClient.ConsolidateGroceryListAsync(items);

            // Assert
            result.Items[0].Category.ShouldBe("Other");
            result.Items[0].Sources.ShouldNotBeNull();
            result.Items[0].Sources.ShouldBeEmpty();
        }

        [Fact]
        public async Task NormalizeEntityAsync_WithRecognizedChef_ReturnsNormalizedEntity() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.Reset().StubNormalizeEntitySuccess(
                isRecognized: true,
                canonicalName: "Bobby Flay",
                normalizedEntityId: "bobby-flay",
                confidence: 0.95m);

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act - sourceType 2 = Chef
            var result = await geminiClient.NormalizeEntityAsync("bobby flay", 2);

            // Assert
            result.ShouldNotBeNull();
            result.IsRecognized.ShouldBeTrue();
            result.CanonicalName.ShouldBe("Bobby Flay");
            result.NormalizedEntityId.ShouldBe("bobby-flay");
            result.Confidence.ShouldBe(0.95m);
        }

        [Fact]
        public async Task NormalizeEntityAsync_WithUnrecognizedEntity_ReturnsNotRecognized() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.Reset().StubNormalizeEntitySuccess(
                isRecognized: false,
                canonicalName: null,
                normalizedEntityId: null,
                confidence: 0.1m);

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.NormalizeEntityAsync("some random name", 2);

            // Assert
            result.ShouldNotBeNull();
            result.IsRecognized.ShouldBeFalse();
            result.NormalizedEntityId.ShouldBeNull();
        }

        [Fact]
        public async Task NormalizeEntityAsync_WithInvalidSourceType_ReturnsNotRecognized() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act - sourceType 1 = Family (not normalized)
            var result = await geminiClient.NormalizeEntityAsync("some name", 1);

            // Assert
            result.ShouldNotBeNull();
            result.IsRecognized.ShouldBeFalse();
        }

        [Fact]
        public async Task NormalizeEntityAsync_WithNullOrEmptyEntityName_ReturnsNotRecognized() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.NormalizeEntityAsync("", 2);

            // Assert
            result.ShouldNotBeNull();
            result.IsRecognized.ShouldBeFalse();
        }

        [Fact]
        public async Task NormalizeEntityAsync_WithRecognizedRestaurant_ReturnsNormalizedEntity() {
            // Arrange
            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns("test-api-key");
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            mockServer.Reset().StubNormalizeEntitySuccess(
                isRecognized: true,
                canonicalName: "The French Laundry",
                normalizedEntityId: "the-french-laundry",
                confidence: 0.98m);

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act - sourceType 3 = Restaurant
            var result = await geminiClient.NormalizeEntityAsync("french laundry", 3);

            // Assert
            result.ShouldNotBeNull();
            result.IsRecognized.ShouldBeTrue();
            result.CanonicalName.ShouldBe("The French Laundry");
            result.NormalizedEntityId.ShouldBe("the-french-laundry");
        }

        /// <summary>
        /// Null disposable for BeginScope mock
        /// </summary>
        private sealed class NullDisposable : IDisposable {
            public void Dispose() {
                // No-op
            }
        }
    }
}
