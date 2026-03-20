using Cortside.Common.BootStrap;
using RecipeVault.BootStrap.Installer;
using RecipeVault.Integrations.VideoDownload;

namespace RecipeVault.BootStrap {
    public class DefaultApplicationBootStrapper : BootStrapper {
        public DefaultApplicationBootStrapper() {
            installers = [
                new RepositoryInstaller(),
                new DomainServiceInstaller(),
                new NutritionInstaller(),
                new DistributedLockInstaller(),
                new GeminiInstaller(),
                new VideoDownloadInstaller(),
                new SupabaseInstaller(),
                new UsdaInstaller(),
                new ResendInstaller(),
                new FacadeInstaller()
            ];
        }
    }
}
