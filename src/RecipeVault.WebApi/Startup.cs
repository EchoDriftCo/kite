using System;
using System.Linq;
using System.Text.Json;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Asp.Versioning.ApiExplorer;
using Cortside.AspNetCore;
using Cortside.AspNetCore.Auditable;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Builder;
using Cortside.AspNetCore.Common;
using Cortside.AspNetCore.EntityFramework;
using Cortside.AspNetCore.Filters;
using Cortside.AspNetCore.Swagger;
using Cortside.Health;
using Cortside.Health.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RecipeVault.BootStrap;
using RecipeVault.Configuration;
using RecipeVault.Data;
using RecipeVault.Health;
using RecipeVault.WebApi.Authentication;
using RecipeVault.WebApi.Installers;
using RecipeVault.WebApi.RateLimiting;
using RecipeVault.WebApi.Services;

namespace RecipeVault.WebApi {
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup : IWebApiStartup {
        private static readonly string[] ValidJwtAlgorithms = { "HS256", "RS256", "ES256" };
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

            services.AddAuthentication(options => {
                    options.DefaultScheme = "ApiTokenOrJwt";
                    options.DefaultChallengeScheme = "ApiTokenOrJwt";
                })
                .AddPolicyScheme("ApiTokenOrJwt", "API Token or JWT", options => {
                    options.ForwardDefaultSelector = context => {
                        var auth = context.Request.Headers.Authorization.ToString();
                        if (auth.StartsWith("Bearer rv_", StringComparison.Ordinal)) {
                            return "ApiToken";
                        }
                        return JwtBearerDefaults.AuthenticationScheme;
                    };
                })
                .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>("ApiToken", null)
                .AddJwtBearer(options => {
                    // Allow HTTP for local development (Supabase runs on http://...)
                    options.RequireHttpsMetadata = !supabaseUrl?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ?? true;

                    // Prevent ASP.NET Core from remapping JWT claim types (e.g. "sub" -> long URI)
                    // so Cortside SubjectPrincipal can find the "sub" claim by its original name
                    options.MapInboundClaims = false;

                    // Configure OIDC discovery using the reachable Supabase URL.
                    // When running inside Docker, the OIDC discovery doc returns 127.0.0.1 URLs
                    // which aren't reachable from the container. We set the JWKS endpoint directly
                    // using the reachable supabaseUrl to bypass this issue.
                    options.Authority = $"{supabaseUrl}/auth/v1";
                    options.Audience = jwtAudience;

                    // Signing keys come straight from the JWKS endpoint, using the reachable
                    // Supabase URL: inside Docker the discovery document advertises 127.0.0.1,
                    // which the container can't reach, so prefer the env var. The configuration
                    // manager fetches lazily on the first token, caches the keys and refetches
                    // when a token arrives with an unknown key id, so a Supabase key rotation
                    // doesn't need a restart and a slow Supabase can't stall startup.
                    // No Supabase URL (integration tests): the tests supply their own key.
                    var reachableSupabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? supabaseUrl;
                    if (!string.IsNullOrWhiteSpace(reachableSupabaseUrl)) {
                        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                            $"{reachableSupabaseUrl}/auth/v1/.well-known/jwks.json",
                            new JwksConfigurationRetriever(jwtIssuer),
                            new HttpDocumentRetriever { RequireHttps = options.RequireHttpsMetadata });
                    }

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        // Supabase signs with ES256 (its JWKS publishes one EC key); HS256 is what
                        // the integration tests sign with
                        ValidAlgorithms = ValidJwtAlgorithms
                    };

                    // Add detailed error logging and claim mapping
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {
                        OnAuthenticationFailed = context => {
                            if (context.Exception.InnerException != null) {
                                Console.WriteLine($"   Inner: {context.Exception.InnerException.Message}");
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context => {
                            // Every write is audited against the subject id, and Cortside falls back
                            // to the shared Guid.Empty "service" subject when it can't parse one
                            if (!Guid.TryParse(context.Principal?.FindFirst("sub")?.Value, out var subjectId) || subjectId == Guid.Empty) {
                                context.Fail("The token's sub claim is not a user id.");
                                return Task.CompletedTask;
                            }

                            // Map Supabase "email" claim to "upn" for Cortside SubjectPrincipal
                            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                            var emailClaim = identity?.FindFirst("email");
                            if (emailClaim != null && identity?.FindFirst("upn") == null) {
                                identity.AddClaim(new System.Security.Claims.Claim("upn", emailClaim.Value));
                            }
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
                var sslEnv = Environment.GetEnvironmentVariable("DATABASE_SSL");
                var ssl = sslEnv == "disable" ? ";SSL Mode=Disable"
                    : (host != "localhost" && host != "127.0.0.1" && host != "host.docker.internal") ? ";SSL Mode=Require;Trust Server Certificate=true" : "";
                connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}{ssl}";
            }
            Configuration["Database:ConnectionString"] = connectionString;
            // Register DbContext with PostgreSQL (Npgsql) instead of Cortside's default SQL Server
            services.AddDbContext<RecipeVaultDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IRecipeVaultDbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<RecipeVaultDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RecipeVaultDbContext>());

            // Add memory cache for substitution caching
            services.AddMemoryCache();

            // Nothing seeds or migrates at startup. Schema and reference data (including the
            // onboarding sample recipes) are applied by migrations in the deploy workflow.

            // add health services
            services.AddHealth(o => {
                o.UseConfiguration(Configuration);
                // The key is the check *Type* in HealthCheckHostedService:Checks, not its Name.
                o.AddCustomCheck("postgres", typeof(PostgresHealthCheck));
                o.AddCustomCheck("gemini", typeof(GeminiHealthCheck));
                o.AddCustomCheck("usda", typeof(UsdaHealthCheck));
            });

            // add controllers and all of the api defaults
            services.AddApiDefaults(InternalDateTimeHandling.Utc, options => {
                options.Filters.Add<MessageExceptionResponseFilter>();
            });

            // add SubjectPrincipal for auditing
            services.AddSubjectPrincipal();
            services.AddTransient<ISubjectFactory<Subject>, SupabaseSubjectFactory>();

            // add CORS (from config or env vars)
            var corsOrigins = Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(",")
                ?? new[] { "http://localhost:4200", "https://localhost:4200" };
            services.AddCors(options => {
                options.AddDefaultPolicy(policy => policy
                    .WithOrigins(corsOrigins)
                    .SetIsOriginAllowed(origin =>
                        corsOrigins.Contains(origin) ||
                        origin.StartsWith("chrome-extension://", StringComparison.Ordinal) ||
                        origin.StartsWith("moz-extension://", StringComparison.Ordinal) ||
                        origin.StartsWith("ms-browser-extension://", StringComparison.Ordinal))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            // add rate limiting, partitioned per caller (see RateLimitPolicies)
            services.AddRateLimiter(options => {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(RateLimitPolicies.Api, context => RateLimitPolicies.FixedWindow(context, permitLimit: 60));
                options.AddPolicy(RateLimitPolicies.Upload, context => RateLimitPolicies.FixedWindow(context, permitLimit: 10));
            });

            // HSTS - 1 year per OWASP recommendation
            services.AddHsts(options => {
                options.MaxAge = TimeSpan.FromSeconds(31536000);
                options.IncludeSubDomains = true;
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

            // Security headers
            app.Use(async (context, next) => {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                // Content-Security-Policy
                // Angular SPA needs: unsafe-inline for styles (Angular uses inline styles),
                // Google Fonts for Roboto + Material Icons, Supabase for API + storage,
                // and Sentry for frontend error reporting.
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "worker-src 'self' blob:; " +
                    "style-src 'self' 'unsafe-inline' fonts.googleapis.com; " +
                    "font-src 'self' fonts.gstatic.com; " +
                    "img-src 'self' data: blob: *.supabase.co; " +
                    "connect-src 'self' *.supabase.co *.sentry.io *.ingest.sentry.io; " +
                    "frame-ancestors 'none';";
                await next().ConfigureAwait(false);
            });
            if (!env.IsDevelopment()) {
                app.UseHsts();
            }

            // Serve static files (Angular SPA + any uploaded assets)
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // order of the following matters
            app.UseCors();
            app.UseAuthentication();
            app.UseSubjectPrincipal(); // intentionally set after UseAuthentication
            app.UseRouting();
            // After UseRouting, or [EnableRateLimiting] policies are never applied: the
            // limiter needs the endpoint to find its policy. After UseAuthentication, so
            // requests can be partitioned per user.
            app.UseRateLimiter();
            app.UseAuthorization(); // intentionally set after UseRouting
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                
                // Map /health to /api/health for standard health check convention
                // Deckard expects /health, Cortside.Health registers /api/health
                endpoints.MapGet("/health", context => {
                    context.Response.Redirect("/api/health");
                    return Task.CompletedTask;
                });

                // Liveness: the process is up and serving. No dependency checks, so it
                // answers immediately after a cold start. Fly's machine check uses this;
                // /api/health stays the readiness check (database, Gemini).
                endpoints.MapGet("/health/live", () => Results.Text("ok"));
                
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
