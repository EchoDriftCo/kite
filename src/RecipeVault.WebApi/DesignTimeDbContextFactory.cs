using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Cortside.AspNetCore.Auditable;
using Cortside.Common.Security;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using RecipeVault.Data;

namespace RecipeVault.WebApi {
    /// <summary>
    /// Design time context factory for EF migrations
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RecipeVaultDbContext> {
        /// <summary>
        /// Create context
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public RecipeVaultDbContext CreateDbContext(string[] args) {
            // Load .env file if it exists
            var envPath = FindEnvFile(Directory.GetCurrentDirectory());
            if (!string.IsNullOrEmpty(envPath)) {
                DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envPath }));
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetSection("Database").GetValue<string>("ConnectionString");

            // Build connection string from env vars if not in appsettings
            if (string.IsNullOrWhiteSpace(connectionString)) {
                var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
                var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
                var database = Environment.GetEnvironmentVariable("DATABASE_NAME");
                var user = Environment.GetEnvironmentVariable("DATABASE_USER");
                var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
                var ssl = host != "localhost" && host != "127.0.0.1" ? ";SSL Mode=Require;Trust Server Certificate=true" : "";
                connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}{ssl}";
            }

            var builder = new DbContextOptionsBuilder<RecipeVaultDbContext>();
            builder.UseNpgsql(connectionString);

            var principal = new SubjectPrincipal(new List<Claim> { new Claim("sub", Guid.Empty.ToString()) });
            return new RecipeVaultDbContext(builder.Options, principal, new DefaultSubjectFactory());
        }

        private static string FindEnvFile(string startDir) {
            var dir = new DirectoryInfo(startDir);
            while (dir != null) {
                var path = Path.Combine(dir.FullName, ".env");
                if (File.Exists(path)) {
                    return path;
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
}
