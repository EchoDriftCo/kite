using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Cortside.AspNetCore.Auditable;
using Cortside.Common.Security;
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
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetSection("Database").GetValue<string>("ConnectionString");

            var builder = new DbContextOptionsBuilder<RecipeVaultDbContext>();
            builder.UseNpgsql(connectionString);

            var principal = new SubjectPrincipal(new List<Claim> { new Claim("sub", Guid.Empty.ToString()) });
            return new RecipeVaultDbContext(builder.Options, principal, new DefaultSubjectFactory());
        }
    }
}
