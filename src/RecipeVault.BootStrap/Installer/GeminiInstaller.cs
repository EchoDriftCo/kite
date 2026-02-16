using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.BootStrap.Installer {
    /// <summary>
    /// Installer for Gemini API integration
    /// </summary>
    public class GeminiInstaller : IInstaller {
        /// <summary>
        /// Configures Gemini client with HttpClientFactory
        /// </summary>
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddHttpClient<IGeminiClient, GeminiClient>(client => {
                client.BaseAddress = new System.Uri("https://generativelanguage.googleapis.com");
                client.Timeout = System.TimeSpan.FromSeconds(60);
            });

            services.AddHttpClient("RecipeUrlFetcher", client => {
                client.Timeout = System.TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RecipeVault/1.0");
            });
        }
    }
}
