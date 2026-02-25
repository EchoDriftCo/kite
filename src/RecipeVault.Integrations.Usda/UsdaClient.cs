using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public class UsdaClient : IUsdaClient {
        private readonly HttpClient httpClient;
        private readonly UsdaConfiguration configuration;
        private readonly ILogger<UsdaClient> logger;

        public UsdaClient(HttpClient httpClient, IOptions<UsdaConfiguration> options, ILogger<UsdaClient> logger) {
            this.httpClient = httpClient;
            this.configuration = options.Value;
            this.logger = logger;

            httpClient.BaseAddress = new Uri(configuration.BaseUrl);
        }

        public bool IsAvailable() {
            return !string.IsNullOrWhiteSpace(configuration.ApiKey);
        }

        public async Task<FoodSearchResponse> SearchFoodsAsync(string query, int pageSize = 10) {
            if (!IsAvailable()) {
                logger.LogWarning("USDA API key not configured");
                return new FoodSearchResponse { Foods = new System.Collections.Generic.List<FoodSearchResult>(), TotalHits = 0 };
            }

            try {
                var response = await httpClient.GetAsync($"/foods/search?query={Uri.EscapeDataString(query)}&pageSize={pageSize}&api_key={configuration.ApiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FoodSearchResponse>(content, new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                });

                logger.LogInformation("USDA search for '{Query}' returned {Count} results", query, result?.Foods?.Count ?? 0);
                return result;
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to search USDA foods for query: {Query}", query);
                throw;
            }
        }

        public async Task<FoodDetails> GetFoodDetailsAsync(int fdcId) {
            if (!IsAvailable()) {
                logger.LogWarning("USDA API key not configured");
                return null;
            }

            try {
                var response = await httpClient.GetAsync($"/food/{fdcId}?api_key={configuration.ApiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FoodDetails>(content, new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                });

                logger.LogInformation("Retrieved USDA food details for FDC ID: {FdcId}", fdcId);
                return result;
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to get USDA food details for FDC ID: {FdcId}", fdcId);
                throw;
            }
        }
    }
}
