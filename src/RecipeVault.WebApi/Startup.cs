using System;
using System.Text;
using Asp.Versioning.ApiExplorer;
using Cortside.AspNetCore;
using Cortside.AspNetCore.Auditable;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Builder;
using Cortside.AspNetCore.Common;
using Cortside.AspNetCore.Filters;
using Cortside.AspNetCore.Swagger;
using Microsoft.EntityFrameworkCore;
using Cortside.Health;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RecipeVault.BootStrap;
using RecipeVault.Configuration;
using RecipeVault.Data;
using RecipeVault.Health;
using RecipeVault.WebApi.Installers;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;

namespace RecipeVault.WebApi {
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup : IWebApiStartup {
        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="configuration"></param>
        [ActivatorUtilitiesConstructor]
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        /// <summary>
        /// Startup
        /// </summary>
        public Startup() {
        }

        /// <summary>
        /// Config
        /// </summary>
        private IConfiguration Configuration { get; set; }

        /// <summary>
        /// Sets the Configuration to be used
        /// </summary>
        /// <param name="config"></param>
        public void UseConfiguration(IConfiguration config) {
            Configuration = config;
        }

        /// <summary>
        /// Configure Services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services) {
            // setup global default json serializer settings
            JsonConvert.DefaultSettings = JsonNetUtility.GlobalDefaultSettings;

            // add Supabase JWT bearer authentication (from config or env vars)
            var supabaseUrl = Configuration["Supabase:Url"] ?? Environment.GetEnvironmentVariable("SUPABASE_URL");
            var jwtIssuer = Configuration["Supabase:Auth:Issuer"] ?? Environment.GetEnvironmentVariable("SUPABASE_JWT_ISSUER");
            var jwtAudience = Configuration["Supabase:Auth:Audience"] ?? Environment.GetEnvironmentVariable("SUPABASE_JWT_AUDIENCE") ?? "authenticated";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    // Use Supabase JWKS endpoint to get public keys for RS256 validation
                    options.Authority = jwtIssuer;
                    options.Audience = jwtAudience;

                    // Prevent ASP.NET Core from remapping JWT claim types (e.g. "sub" → long URI)
                    // so Cortside SubjectPrincipal can find the "sub" claim by its original name
                    options.MapInboundClaims = false;

                    // Configure JWKS endpoint
                    options.MetadataAddress = $"{supabaseUrl}/auth/v1/.well-known/openid-configuration";

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };

                    // Add detailed error logging
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {
                        OnAuthenticationFailed = context => {
                            if (context.Exception.InnerException != null) {
                                Console.WriteLine($"   Inner: {context.Exception.InnerException.Message}");
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context => {
                            return Task.CompletedTask;
                        },
                        OnChallenge = context => {
                            return Task.CompletedTask;
                        }
                    };
                });

            // add database context with interfaces
            // Build connection string from env vars if not in appsettings
            var connectionString = Configuration.GetConnectionString("DefaultConnection")
                ?? Configuration["Database:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString)) {
                var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
                var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
                var database = Environment.GetEnvironmentVariable("DATABASE_NAME");
                var user = Environment.GetEnvironmentVariable("DATABASE_USER");
                var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
                var ssl = host != "localhost" && host != "127.0.0.1" ? ";SSL Mode=Require;Trust Server Certificate=true" : "";
                connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}{ssl}";
            }
            Configuration["Database:ConnectionString"] = connectionString;
            // Register DbContext with PostgreSQL (Npgsql) instead of Cortside's default SQL Server
            services.AddDbContext<RecipeVaultDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IRecipeVaultDbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RecipeVaultDbContext>());

            // add health services
            services.AddHealth(o => {
                o.UseConfiguration(Configuration);
                o.AddCustomCheck("postgres", typeof(PostgresHealthCheck));
                o.AddCustomCheck("gemini", typeof(GeminiHealthCheck));
            });

            // add controllers and all of the api defaults
            services.AddApiDefaults(InternalDateTimeHandling.Utc, options => {
                options.Filters.Add<MessageExceptionResponseFilter>();
            });

            // add SubjectPrincipal for auditing
            services.AddSubjectPrincipal();
            services.AddTransient<ISubjectFactory<Subject>, DefaultSubjectFactory>();

            // add CORS (from config or env vars)
            var corsOrigins = Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(",")
                ?? new[] { "http://localhost:4200", "https://localhost:4200" };
            services.AddCors(options => {
                options.AddDefaultPolicy(policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod());
            });

            // Add swagger with versioning
            services.AddSwagger(Configuration, "RecipeVault API", "RecipeVault API", ["v1"]);

            // setup and register bootstrapper and its installers
            services.AddBootStrapper<DefaultApplicationBootStrapper>(Configuration, o => {
                o.AddInstaller(new ModelMapperInstaller());
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="provider"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider) {
            app.UseApiDefaults(Configuration);
            app.UseSwagger("RecipeVault Api", provider);

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // Serve uploaded images from wwwroot
            app.UseStaticFiles();

            // order of the following matters
            app.UseCors();
            app.UseAuthentication();
            app.UseSubjectPrincipal(); // intentionally set after UseAuthentication
            app.UseRouting();
            app.UseAuthorization(); // intentionally set after UseRouting
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
