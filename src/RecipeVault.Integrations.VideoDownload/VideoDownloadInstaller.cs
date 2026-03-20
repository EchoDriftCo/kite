using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cortside.Common.BootStrap;

namespace RecipeVault.Integrations.VideoDownload {
    /// <summary>
    /// Installer for VideoDownload integration
    /// </summary>
    public class VideoDownloadInstaller : IInstaller {
        /// <summary>
        /// Configures VideoDownloadService
        /// </summary>
        public void Install(IServiceCollection services, IConfiguration configuration) {
            services.AddTransient<IVideoDownloadService, VideoDownloadService>();
        }
    }
}
