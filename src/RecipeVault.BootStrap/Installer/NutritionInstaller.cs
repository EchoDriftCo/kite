using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.DomainService;

namespace RecipeVault.BootStrap.Installer {
    public class NutritionInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddScoped<INutritionService, NutritionService>();
        }
    }
}
