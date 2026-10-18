using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Health;
using Cortside.Health.Checks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using RecipeVault.WebApi.IntegrationTests.Base;

namespace RecipeVault.WebApi.IntegrationTests.Health {
    /// <summary>
    /// Every check in HealthCheckHostedService:Checks names a Type that must match a
    /// key passed to AddCustomCheck. When it doesn't, Cortside quietly substitutes an
    /// UnresolvedCheck that fails with "Could not find check in registered checks" —
    /// which is how the database check went unnoticed in production.
    /// </summary>
    public class HealthCheckRegistrationTests : IAsyncLifetime, IDisposable {
        private readonly IntegrationFixture _fixture;

        public HealthCheckRegistrationTests() {
            _fixture = new IntegrationFixture();
        }

        public Task InitializeAsync() => _fixture.InitializeAsync();

        public Task DisposeAsync() => _fixture.DisposeAsync();

        [Fact]
        public void EveryConfiguredCheck_ResolvesToARegisteredType() {
            var configuration = _fixture.Services.GetRequiredService<IConfiguration>();
            var checks = configuration.GetSection("HealthCheckHostedService:Checks").Get<List<CheckConfiguration>>();
            checks.ShouldNotBeEmpty();

            var factory = _fixture.Services.GetRequiredService<ICheckFactory>();
            foreach (var check in checks) {
                factory.Create(check).ShouldNotBeOfType<UnresolvedCheck>(
                    $"health check '{check.Name}' has Type '{check.Type}', which no AddCustomCheck registers");
            }
        }

        /// <summary>
        /// Cortside re-runs a check only once its last result is at least Interval old, so a check
        /// that narrowly misses one round refreshes a full round later: up to 2 x Interval after
        /// its last result, plus up to Timeout to finish. If the cached result expires before
        /// that, the aggregate (computed in the same round, and faster) reads an empty cache and
        /// /api/health reports "status not cached". With the 60s default against a 30s interval,
        /// production flapped between 200 and 503 every 30 seconds.
        /// </summary>
        [Fact]
        public void EveryCheckResult_OutlivesASkippedRound() {
            var configuration = _fixture.Services.GetRequiredService<IConfiguration>();
            var service = configuration.GetSection("HealthCheckHostedService").Get<HealthCheckServiceConfiguration>();

            // The aggregate check runs on the service's settings, with Timeout = Interval
            var entries = new List<(string Name, int Interval, int Timeout, int CacheDuration)> {
                (service.Name, service.Interval, service.Interval, service.CacheDuration)
            };
            entries.AddRange(service.Checks.Select(c => (c.Name, c.Interval, c.Timeout, c.CacheDuration)));

            foreach (var (name, interval, timeout, cacheDuration) in entries) {
                // Cortside's defaults: no CacheDuration (or one below Interval) means 2 x Interval,
                // and no Timeout means Interval
                var effectiveCache = cacheDuration < interval ? interval * 2 : cacheDuration;
                var effectiveTimeout = timeout == 0 ? interval : timeout;
                effectiveCache.ShouldBeGreaterThan(2 * interval + effectiveTimeout,
                    $"'{name}': a {effectiveCache}s cache can expire before a {interval}s-interval check refreshes");
            }
        }

        public void Dispose() {
            _fixture.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
