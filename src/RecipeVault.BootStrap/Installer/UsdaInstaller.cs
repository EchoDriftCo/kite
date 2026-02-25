using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Integrations.Usda;

namespace RecipeVault.BootStrap.Installer {
    public class UsdaInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.Configure<UsdaConfiguration>(configuration.GetSection("Usda"));
            services.AddHttpClient<IUsdaClient, UsdaClient>();
        }
    }
}
