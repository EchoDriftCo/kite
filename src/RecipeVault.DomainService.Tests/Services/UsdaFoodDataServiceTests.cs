using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using RecipeVault.Integrations.Usda;
using Xunit;

namespace RecipeVault.DomainService.Tests.Services {
    public class UsdaFoodDataServiceTests {
        [Fact]
        public async Task SearchFoodsAsync_WithConfiguredApiKey_WhenUsdaFails_ShouldThrowHttpRequestException() {
            // Arrange
            var service = CreateService(
                apiKey: "test-key",
                handler: new ThrowingHandler(new HttpRequestException("USDA down"))
            );

            // Act & Assert
            await Should.ThrowAsync<HttpRequestException>(() => service.SearchFoodsAsync("chicken"));
        }

        [Fact]
        public async Task GetFoodDetailsAsync_WithConfiguredApiKey_WhenUsdaFails_ShouldThrowHttpRequestException() {
            // Arrange
            var service = CreateService(
                apiKey: "test-key",
                handler: new ThrowingHandler(new TaskCanceledException("timeout"))
            );

            // Act & Assert
            await Should.ThrowAsync<HttpRequestException>(() => service.GetFoodDetailsAsync(173851));
        }

        [Fact]
        public async Task SearchFoodsAsync_WithoutApiKey_ShouldReturnMockData() {
            // Arrange
            var service = CreateService(apiKey: string.Empty, handler: new ThrowingHandler(new InvalidOperationException("ignored")));

            // Act
            var result = await service.SearchFoodsAsync("rice");

            // Assert
            result.ShouldNotBeNull();
            result.Foods.ShouldNotBeNull();
            result.Foods.Count.ShouldBeGreaterThan(0);
        }

        private static UsdaFoodDataService CreateService(string apiKey, HttpMessageHandler handler) {
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.nal.usda.gov/fdc/v1") };
            var options = Options.Create(new UsdaConfiguration {
                ApiKey = apiKey,
                BaseUrl = "https://api.nal.usda.gov/fdc/v1"
            });
            var logger = new Mock<ILogger<UsdaFoodDataService>>();

            return new UsdaFoodDataService(httpClient, options, logger.Object);
        }

        private sealed class ThrowingHandler : HttpMessageHandler {
            private readonly Exception exception;

            public ThrowingHandler(Exception exception) {
                this.exception = exception;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
                return Task.FromException<HttpResponseMessage>(exception);
            }
        }
    }
}
