using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using RecipeVault.Data;
using RecipeVault.Integrations.Gemini;
using RecipeVault.Integrations.Gemini.Tests.Mocks;

namespace RecipeVault.WebApi.IntegrationTests.Base {
    public class IntegrationFixture : IAsyncLifetime, IDisposable {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly GeminiMockServer _geminiMockServer;
        private bool _disposed;
        public HttpClient HttpClient { get; private set; }
        public IServiceProvider Services { get; private set; }
        public GeminiMockServer GeminiMockServer => _geminiMockServer;

        // Must match the value in AuthenticationHelper.TestSecret
        private const string TestJwtSecret = "bUWJ9VWAx24G11fJOornAd1YeKRTokg8SV3IftX/WH4MFQlknXzaKGY7qO2fNhU4S7shnPS9TLf8t9Z/My0X/g==";

        public IntegrationFixture() {
            // Create Gemini mock server
            _geminiMockServer = new GeminiMockServer();

            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder => {
                    // Configure test settings including JWT secret
                    builder.ConfigureAppConfiguration((context, config) => {
                        config.AddInMemoryCollection(new Dictionary<string, string> {
                            ["Supabase:Auth:JwtSecret"] = TestJwtSecret,
                            ["Supabase:Auth:Issuer"] = "https://umwycxfebintkenehqlj.supabase.co/auth/v1",
                            ["Supabase:Auth:Audience"] = "authenticated",
                            ["Gemini:ApiKey"] = "test-api-key",
                            ["Gemini:Model"] = "gemini-1.5-flash"
                        });
                    });

                    builder.ConfigureServices(services => {
                        // Remove all DbContext registrations
                        var dbContextDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<RecipeVaultDbContext>));
                        if (dbContextDescriptor != null) {
                            services.Remove(dbContextDescriptor);
                        }

                        // Remove any other DbContext options registrations
                        services.RemoveAll(typeof(DbContextOptions));
                        services.RemoveAll(typeof(DbContextOptions<RecipeVaultDbContext>));

                        // Remove the IDbContextOptionsConfiguration registrations
                        var dbConfigDescriptors = services
                            .Where(d => d.ServiceType.Name.Contains("DbContextOptions"))
                            .ToList();
                        foreach (var descriptor in dbConfigDescriptors) {
                            services.Remove(descriptor);
                        }

                        // Add test database with InMemory provider
                        var uniqueDbName = "IntegrationTestDb_" + Guid.NewGuid();
                        services.AddDbContext<RecipeVaultDbContext>((sp, options) => {
                            options.UseInMemoryDatabase(uniqueDbName);
                            // Suppress transaction warning since InMemory doesn't support transactions
                            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                        });

                        // Register IRecipeVaultDbContext for the test
                        services.AddScoped<IRecipeVaultDbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
                        services.AddScoped<DbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());

                        // Replace the Gemini HttpClient with one pointing to our mock server
                        services.RemoveAll<IGeminiClient>();
                        services.AddHttpClient<IGeminiClient, GeminiClient>(client => {
                            client.BaseAddress = new Uri(_geminiMockServer.Url);
                            client.Timeout = TimeSpan.FromSeconds(30);
                        });

                        // Reconfigure JWT authentication to use HS256 with test secret
                        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options => {
                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
                            options.TokenValidationParameters = new TokenValidationParameters {
                                ValidateIssuer = true,
                                ValidIssuer = "https://umwycxfebintkenehqlj.supabase.co/auth/v1",
                                ValidateAudience = true,
                                ValidAudience = "authenticated",
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = key,
                                ValidateLifetime = true
                            };
                            // Disable the automatic key discovery since we're using a symmetric key
                            options.RequireHttpsMetadata = false;
                            options.MetadataAddress = null;
                            options.Authority = null;
                        });
                    });
                });

            HttpClient = _factory.CreateClient();
            Services = _factory.Services;
        }

        public async Task InitializeAsync() {
            // Setup default Gemini mock responses
            _geminiMockServer.StubParseRecipeSuccess();

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
            _geminiMockServer?.Dispose();
        }

        /// <summary>
        /// Reset the Gemini mock server and configure a new stub
        /// </summary>
        public void ResetGeminiMock() {
            _geminiMockServer.Reset();
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

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    HttpClient?.Dispose();
                    _factory?.Dispose();
                    _geminiMockServer?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
