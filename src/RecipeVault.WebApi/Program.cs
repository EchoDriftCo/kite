using System.IO;
using System.Threading.Tasks;
using Cortside.AspNetCore.Builder;
using dotenv.net;

namespace RecipeVault.WebApi {
    /// <summary>
    /// Program
    /// </summary>
    public static class Program {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<int> Main(string[] args) {
            // Load .env file if it exists (development only)
            // Search current directory and parent directories for .env
            var currentDir = Directory.GetCurrentDirectory();
            var envPath = FindEnvFile(currentDir);
            if (!string.IsNullOrEmpty(envPath)) {
                DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envPath }));
            }

            var builder = WebApiHost.CreateBuilder(args)
                .UseStartup<Startup>();

            var api = builder.Build();
            return api.StartAsync();
        }

        private static string FindEnvFile(string startDir) {
            var dir = new DirectoryInfo(startDir);
            while (dir != null) {
                var envPath = Path.Combine(dir.FullName, ".env");
                if (File.Exists(envPath)) {
                    return envPath;
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
}
