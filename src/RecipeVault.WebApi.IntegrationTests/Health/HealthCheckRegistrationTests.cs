using System;
using System.Collections.Generic;
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

        public void Dispose() {
            _fixture.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
