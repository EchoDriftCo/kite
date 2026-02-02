using Cortside.Common.BootStrap;
using RecipeVault.BootStrap.Installer;

namespace RecipeVault.BootStrap {
    public class DefaultApplicationBootStrapper : BootStrapper {
        public DefaultApplicationBootStrapper() {
            installers = [
                new RepositoryInstaller(),
                new DomainServiceInstaller(),
                new DistributedLockInstaller(),
                new GeminiInstaller(),
                new FacadeInstaller()
            ];
        }
    }
}
