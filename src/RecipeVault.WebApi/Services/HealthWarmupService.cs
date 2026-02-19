using System;
using System.Threading;
using System.Threading.Tasks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RecipeVault.WebApi.Services {
    /// <summary>
    /// Seeds the health check cache on startup so that cold starts don't trigger
    /// false 503 alarms from uptime monitors before background checks have run.
    ///
    /// On first request after a cold start, Cortside.Health returns 503 because:
    ///   1. HealthCheckHostedService uses await Task.Yield() so the app accepts
    ///      requests before the first check batch completes.
    ///   2. HealthCheck (the aggregate) runs in parallel with individual checks
    ///      and often finishes first (it's in-memory), reading empty sub-caches
    ///      which resolve to Failure("status not cached").
    ///
    /// This service seeds each configured check with a "Starting up" result so
    /// the aggregate computes Healthy=true immediately. Real results replace the
    /// seeds once the background checks run (within their configured interval).
    /// </summary>
    public class HealthWarmupService : IHostedService {
        private readonly IMemoryCache cache;
        private readonly IConfiguration configuration;
        private readonly ILogger<HealthWarmupService> logger;

        public HealthWarmupService(
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<HealthWarmupService> logger) {
            this.cache = cache;
            this.configuration = configuration;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            // Use Timestamp = UtcNow so InternalExecuteAsync sees age < Interval
            // and skips running the real check until the configured interval passes.
            // TTL is generous (60s); real results overwrite this within ~30s.
            var now = DateTime.UtcNow;
            var ttl = DateTimeOffset.UtcNow.AddSeconds(60);

            var checks = configuration.GetSection("HealthCheckHostedService:Checks").GetChildren();
            foreach (var checkConfig in checks) {
                var name = checkConfig["Name"];
                if (string.IsNullOrWhiteSpace(name)) {
                    continue;
                }

                var required = bool.TryParse(checkConfig["Required"], out var req) && req;

                // Only seed if nothing is cached yet — don't overwrite a real result
                if (cache.Get(name) != null) {
                    continue;
                }

                var seed = new ServiceStatusModel {
                    Healthy = true,
                    Status = ServiceStatus.Ok,
                    StatusDetail = "Starting up",
                    Required = required,
                    Timestamp = now
                };

                cache.Set(name, seed, ttl);
                logger.LogInformation(
                    "HealthWarmupService: seeded {CheckName} (required={Required}) with startup status",
                    name, required);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
