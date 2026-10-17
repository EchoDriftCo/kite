using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Facade;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.BootStrap.Installer {
    public class FacadeInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddScopedInterfacesBySuffix<RecipeFacade>("Facade");
            services.AddSingletonClassesBySuffix<RecipeMapper>("Mapper");
            services.AddScoped<ICookingLogFacade, CookingLogFacade>();
            services.AddSingleton<CookingLogMapper>();
            services.AddScoped<IPremiumWaitlistFacade, PremiumWaitlistFacade>();
        }
    }
}
