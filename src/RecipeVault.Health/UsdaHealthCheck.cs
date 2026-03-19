using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cortside.Health;
using Cortside.Health.Checks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RecipeVault.Health {
    public class UsdaHealthCheck : Check {
        private readonly string apiKey;
        private readonly IHttpClientFactory httpClientFactory;

        public UsdaHealthCheck(IMemoryCache cache, ILogger<UsdaHealthCheck> logger, IAvailabilityRecorder recorder, IConfiguration configuration, IHttpClientFactory httpClientFactory) : base(cache, logger, recorder) {
            apiKey = configuration["Usda:ApiKey"];
            this.httpClientFactory = httpClientFactory;
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            try {
                // Simple connectivity check to USDA FoodData Central API
                // Use a lightweight endpoint that doesn't consume quota unnecessarily
                var httpClient = httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                // FDC API root endpoint - validates API key and connectivity
                var response = await httpClient.GetAsync($"https://api.nal.usda.gov/fdc/v1/foods/list?api_key={apiKey}&pageSize=1").ConfigureAwait(false);

                return new ServiceStatusModel {
                    Healthy = response.IsSuccessStatusCode,
                    Status = response.IsSuccessStatusCode ? ServiceStatus.Ok : ServiceStatus.Degraded,
                    StatusDetail = response.IsSuccessStatusCode ? "USDA FoodData Central API reachable" : $"USDA API returned {response.StatusCode}",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex) {
                return new ServiceStatusModel {
                    Healthy = false,
                    Status = ServiceStatus.Failure,
                    StatusDetail = $"USDA API unreachable: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
