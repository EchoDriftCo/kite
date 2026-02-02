using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RecipeVault.Data;
using RecipeVault.WebApi;

namespace RecipeVault.WebApi.IntegrationTests.Base {
    public class IntegrationFixture : IAsyncLifetime {
        private readonly WebApplicationFactory<Startup> _factory;
        public HttpClient HttpClient { get; private set; }
        public IServiceProvider Services { get; private set; }

        public IntegrationFixture() {
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder => {
                    builder.ConfigureServices(services => {
                        // Remove the app's DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<RecipeVaultDbContext>));

                        if (descriptor != null) {
                            services.Remove(descriptor);
                        }

                        // Add test database
                        services.AddDbContext<RecipeVaultDbContext>(options => {
                            options.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid(), opt => { });
                        });

                        // Register IRecipeVaultDbContext for the test
                        services.AddScoped<IRecipeVaultDbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
                        services.AddScoped<DbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
                    });
                });

            HttpClient = _factory.CreateClient();
            Services = _factory.Services;
        }

        public async Task InitializeAsync() {
            using (var scope = Services.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<RecipeVaultDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
            }
        }

        public async Task DisposeAsync() {
            using (var scope = Services.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<RecipeVaultDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
            }

            HttpClient?.Dispose();
            _factory?.Dispose();
        }

        public async Task<TEntity> AddToDbAsync<TEntity>(TEntity entity) where TEntity : class {
            using (var scope = Services.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<RecipeVaultDbContext>();
                await dbContext.AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }

            return entity;
        }

        public async Task<TEntity> GetFromDbAsync<TEntity>(object id) where TEntity : class {
            using (var scope = Services.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<RecipeVaultDbContext>();
                return await dbContext.FindAsync<TEntity>(id);
            }
        }
    }
}
