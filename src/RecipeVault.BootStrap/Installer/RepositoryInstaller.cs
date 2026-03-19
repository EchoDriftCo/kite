using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Data.Repositories;

namespace RecipeVault.BootStrap.Installer {
    public class RepositoryInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddScopedInterfacesBySuffix<RecipeRepository>("Repository");
            services.AddScoped<ICookingLogRepository, CookingLogRepository>();
        }
    }
}
