using System;
using System.Collections.Generic;
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

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns((string)null);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var geminiClient = new GeminiClient(httpClient, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, mimeType));

            ex.Message.ShouldContain("API key");
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
