using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class ImportJobRepository : IImportJobRepository {
        private readonly IRecipeVaultDbContext context;

        public ImportJobRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(ImportJob job) {
            context.ImportJobs.Add(job);
        }

        public async Task<ImportJob> GetByResourceIdAsync(Guid resourceId) {
            return await context.ImportJobs
                .Where(j => j.ImportJobResourceId == resourceId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
    }
}
