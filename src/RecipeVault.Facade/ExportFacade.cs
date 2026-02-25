using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;

namespace RecipeVault.Facade {
    public class ExportFacade : IExportFacade {
        private readonly IUnitOfWork uow;
        private readonly IExportService exportService;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly ILogger<ExportFacade> logger;

        public ExportFacade(
            ILogger<ExportFacade> logger,
            IUnitOfWork uow,
            IExportService exportService,
            ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.exportService = exportService;
            this.subjectPrincipal = subjectPrincipal;
            this.logger = logger;
        }

        public async Task<string> ExportRecipeAsJsonAsync(Guid recipeResourceId) {
            // Read-only operation, no transaction needed
            await using (var tx = uow.BeginNoTracking()) {
                logger.LogInformation("Exporting recipe {RecipeResourceId} as JSON", recipeResourceId);
                return await exportService.ExportRecipeAsJsonAsync(recipeResourceId).ConfigureAwait(false);
            }
        }

        public async Task<string> ExportRecipeAsTextAsync(Guid recipeResourceId) {
            // Read-only operation, no transaction needed
            await using (var tx = uow.BeginNoTracking()) {
                logger.LogInformation("Exporting recipe {RecipeResourceId} as text", recipeResourceId);
                return await exportService.ExportRecipeAsTextAsync(recipeResourceId).ConfigureAwait(false);
            }
        }

        public async Task<byte[]> ExportRecipeAsPaprikaAsync(Guid recipeResourceId) {
            // Read-only operation, no transaction needed
            await using (var tx = uow.BeginNoTracking()) {
                logger.LogInformation("Exporting recipe {RecipeResourceId} as Paprika", recipeResourceId);
                return await exportService.ExportRecipeAsPaprikaAsync(recipeResourceId).ConfigureAwait(false);
            }
        }

        public async Task<byte[]> ExportAllAsPaprikaAsync() {
            // Read-only operation, no transaction needed
            await using (var tx = uow.BeginNoTracking()) {
                var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
                logger.LogInformation("Exporting all recipes for subject {SubjectId} as Paprika", currentSubjectId);
                return await exportService.ExportAllAsPaprikaAsync(currentSubjectId).ConfigureAwait(false);
            }
        }
    }
}
