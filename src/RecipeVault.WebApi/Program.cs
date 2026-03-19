using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cortside.AspNetCore.Builder;
using dotenv.net;
using Sentry;

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

            // Initialize Sentry for error tracking (before app starts)
            var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
            if (!string.IsNullOrEmpty(sentryDsn)) {
                SentrySdk.Init(options => {
                    options.Dsn = sentryDsn;
                    options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                    options.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                    options.SendDefaultPii = false;
                    options.AttachStacktrace = true;
                    // Disable tracing since we're using basic SDK init
                    options.TracesSampleRate = 0;
                });
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
