using System;
using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Configuration;
using RecipeVault.DomainService;
using RecipeVault.Integrations.Supabase;

namespace RecipeVault.BootStrap.Installer {
    public class SupabaseInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.Configure<SupabaseConfiguration>(configuration.GetSection("Supabase"));

            // Env vars use flat naming (SUPABASE_URL) not hierarchical (Supabase__Url),
            // so PostConfigure fills in any values missing from appsettings
            services.PostConfigure<SupabaseConfiguration>(config => {
                if (string.IsNullOrEmpty(config.Url)) {
                    config.Url = Environment.GetEnvironmentVariable("SUPABASE_URL");
                }
                if (string.IsNullOrEmpty(config.ServiceKey)) {
                    config.ServiceKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_KEY");
                }
            });

            var supabaseUrl = configuration["Supabase:Url"];
            if (string.IsNullOrEmpty(supabaseUrl)) {
                supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            }

            services.AddHttpClient<IImageStorage, SupabaseStorageClient>(client => {
                if (!string.IsNullOrEmpty(supabaseUrl)) {
                    client.BaseAddress = new Uri(supabaseUrl);
                }
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }
    }
}
