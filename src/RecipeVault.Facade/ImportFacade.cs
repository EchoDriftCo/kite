using System.IO;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public class ImportFacade : IImportFacade {
        private readonly IUnitOfWork uow;
        private readonly IImportService importService;
        private readonly ILogger<ImportFacade> logger;

        public ImportFacade(ILogger<ImportFacade> logger, IUnitOfWork uow, IImportService importService) {
            this.uow = uow;
            this.importService = importService;
            this.logger = logger;
        }

        public async Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream) {
            logger.LogInformation("Starting Paprika import");
            
            var result = await importService.ImportFromPaprikaAsync(fileStream).ConfigureAwait(false);
            
            // Save all imported recipes in a single transaction
            await uow.SaveChangesAsync().ConfigureAwait(false);
            
            logger.LogInformation("Paprika import completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                result.SuccessCount, result.FailureCount);
            
            return result;
        }
    }
}
