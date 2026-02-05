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
    public class GeminiHealthCheck : Check {
        private readonly string apiKey;
        private readonly IHttpClientFactory httpClientFactory;

        public GeminiHealthCheck(IMemoryCache cache, ILogger<GeminiHealthCheck> logger, IAvailabilityRecorder recorder, IConfiguration configuration, IHttpClientFactory httpClientFactory) : base(cache, logger, recorder) {
            apiKey = configuration["Gemini:ApiKey"];
            this.httpClientFactory = httpClientFactory;
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            try {
                var httpClient = httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await httpClient.GetAsync($"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}").ConfigureAwait(false);

                return new ServiceStatusModel {
                    Healthy = response.IsSuccessStatusCode,
                    Status = response.IsSuccessStatusCode ? ServiceStatus.Ok : ServiceStatus.Degraded,
                    StatusDetail = response.IsSuccessStatusCode ? "Gemini API reachable" : $"Gemini API returned {response.StatusCode}",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex) {
                return new ServiceStatusModel {
                    Healthy = false,
                    Status = ServiceStatus.Failure,
                    StatusDetail = $"Gemini API unreachable: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
