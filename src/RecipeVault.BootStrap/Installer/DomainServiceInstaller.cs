using Cortside.Common.BootStrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.DomainService;

namespace RecipeVault.BootStrap.Installer {
    public class DomainServiceInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddScopedInterfacesBySuffix<RecipeService>("Service");
        }
    }
}
