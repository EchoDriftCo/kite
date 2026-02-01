using System.Text;
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

            // add Supabase JWT bearer authentication
            var supabase = Configuration.GetSection("Supabase").Get<SupabaseConfiguration>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = supabase.Auth.Issuer,
                        ValidateAudience = true,
                        ValidAudience = supabase.Auth.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabase.Auth.JwtSecret)),
                        ValidateLifetime = true
                    };
                });

            // add database context with interfaces
            services.AddDatabaseContext<IRecipeVaultDbContext, RecipeVaultDbContext>(Configuration);

            // add health services
            services.AddHealth(o => {
                o.UseConfiguration(Configuration);
                o.AddCustomCheck("gemini", typeof(GeminiHealthCheck));
            });

            // add controllers and all of the api defaults
            services.AddApiDefaults(InternalDateTimeHandling.Utc, options => {
                options.Filters.Add<MessageExceptionResponseFilter>();
            });

            // add SubjectPrincipal for auditing
            services.AddSubjectPrincipal();
            services.AddTransient<ISubjectFactory<Subject>, DefaultSubjectFactory>();

            // add CORS
            var origins = Configuration.GetSection("Cors:Origins").Get<string[]>();
            services.AddCors(options => {
                options.AddDefaultPolicy(policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
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
