using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace RecipeVault.Integrations.Gemini.Tests {
    public class GeminiClientTests {
        private readonly Mock<IHttpClientFactory> httpClientFactoryMock;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly Mock<ILogger<GeminiClient>> loggerMock;
        private readonly HttpClient httpClient;

        public GeminiClientTests() {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            configurationMock = new Mock<IConfiguration>();
            loggerMock = new Mock<ILogger<GeminiClient>>();
            httpClient = new HttpClient();
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

            var responseContent = new StringContent("""
                {
                  "candidates": [
                    {
                      "content": {
                        "parts": [
                          {
                            "text": "{\\"title\\": \\"Chocolate Cake\\", \\"yield\\": 8, \\"prepTimeMinutes\\": 15, \\"cookTimeMinutes\\": 30, \\"ingredients\\": [{
                              \\"quantity\\": 2,
                              \\"unit\\": \\"cup\\",
                              \\"item\\": \\"flour\\",
                              \\"preparation\\": null,
                              \\"rawText\\": \\"2 cups flour\\"
                            }], \\"instructions\\": [{
                              \\"stepNumber\\": 1,
                              \\"instruction\\": \\"Preheat oven to 350°F\\",
                              \\"rawText\\": \\"Preheat oven to 350°F\\"
                            }]}"
                          }
                        ]
                      }
                    }
                  ]
                }
                """);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var client = new HttpClient(handlerMock.Object) {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com")
            };

            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var geminiClient = new GeminiClient(client, configurationMock.Object, loggerMock.Object);

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
        public async Task ParseRecipeAsync_WithNoRecipeDetected_Returns422StatusCode() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var client = new HttpClient(new Mock<HttpMessageHandler>().Object) {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com")
            };

            var geminiClient = new GeminiClient(client, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, mimeType));

            ex.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task ParseRecipeAsync_WithGeminiApiDown_Returns503StatusCode() {
            // Arrange
            var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
            var mimeType = "image/png";
            var apiKey = "test-api-key";

            configurationMock.Setup(c => c["Gemini:ApiKey"]).Returns(apiKey);
            configurationMock.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var client = new HttpClient(new Mock<HttpMessageHandler>().Object) {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com")
            };

            var geminiClient = new GeminiClient(client, configurationMock.Object, loggerMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GeminiApiException>(() =>
                geminiClient.ParseRecipeAsync(imageBase64, mimeType));

            ex.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
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

            var responseContent = new StringContent("""
                {
                  "candidates": [
                    {
                      "content": {
                        "parts": [
                          {
                            "text": "{\\"title\\": \\"Chocolate Cake\\", \\"yield\\": 8, \\"prepTimeMinutes\\": 15, \\"cookTimeMinutes\\": 30, \\"ingredients\\": [{
                              \\"quantity\\": 2,
                              \\"unit\\": \\"cup\\",
                              \\"item\\": \\"flour\\",
                              \\"preparation\\": null,
                              \\"rawText\\": \\"2 cups flour\\"
                            }], \\"instructions\\": [{
                              \\"stepNumber\\": 1,
                              \\"instruction\\": \\"Preheat oven to 350°F\\",
                              \\"rawText\\": \\"Preheat oven to 350°F\\"
                            }]}"
                          }
                        ]
                      }
                    }
                  ]
                }
                """);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var client = new HttpClient(handlerMock.Object) {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com")
            };

            var geminiClient = new GeminiClient(client, configurationMock.Object, loggerMock.Object);

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

            var responseContent = new StringContent("""
                {
                  "candidates": [
                    {
                      "content": {
                        "parts": [
                          {
                            "text": "{\\"title\\": \\"Chocolate Cake\\", \\"yield\\": null, \\"prepTimeMinutes\\": null, \\"cookTimeMinutes\\": null, \\"ingredients\\": [], \\"instructions\\": []}"
                          }
                        ]
                      }
                    }
                  ]
                }
                """);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var client = new HttpClient(handlerMock.Object) {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com")
            };

            var geminiClient = new GeminiClient(client, configurationMock.Object, loggerMock.Object);

            // Act
            var result = await geminiClient.ParseRecipeAsync(imageBase64, mimeType);

            // Assert
            result.Warnings.ShouldNotBeEmpty();
            result.Confidence.ShouldBeLessThan(1);
        }
    }
}
