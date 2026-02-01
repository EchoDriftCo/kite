using Cortside.Common.BootStrap;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RecipeVault.BootStrap.Installer {
    public class DistributedLockInstaller : IInstaller {
        public void Install(IServiceCollection services, IConfiguration configuration) {
            var connectionString = configuration["Database:ConnectionString"];
            services.AddSingleton<IDistributedLockProvider>(
                new PostgresDistributedSynchronizationProvider(connectionString));
        }
    }
}
