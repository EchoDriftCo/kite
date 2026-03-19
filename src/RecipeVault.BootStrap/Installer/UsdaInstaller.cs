using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Integrations.Usda;

namespace RecipeVault.BootStrap.Installer {
    public class UsdaInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            // ASP.NET Core auto-maps env var Usda__ApiKey to Usda:ApiKey config
            services.Configure<UsdaConfiguration>(configuration.GetSection("Usda"));
            services.AddHttpClient<IUsdaFoodDataService>();
            services.AddScoped<IUsdaFoodDataService, UsdaFoodDataService>();
            services.AddScoped<IIngredientParser, IngredientParser>();
            services.AddScoped<IUnitConverter, UnitConverter>();
        }
    }
}
