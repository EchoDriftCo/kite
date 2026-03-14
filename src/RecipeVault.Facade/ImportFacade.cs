using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class ImportFacade : IImportFacade {
        private readonly IUnitOfWork uow;
        private readonly IImportService importService;
        private readonly RecipeMapper recipeMapper;
        private readonly ILogger<ImportFacade> logger;

        public ImportFacade(ILogger<ImportFacade> logger, IUnitOfWork uow, IImportService importService, RecipeMapper recipeMapper) {
            this.uow = uow;
            this.importService = importService;
            this.recipeMapper = recipeMapper;
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

        public async Task<RecipeDto> ImportFromUrlAsync(string url) {
            logger.LogInformation("Starting URL import from: {Url}", url);
            
            var recipe = await importService.ImportFromUrlAsync(url).ConfigureAwait(false);
            
            // Save the imported recipe
            await uow.SaveChangesAsync().ConfigureAwait(false);
            
            logger.LogInformation("URL import completed: {Title}", recipe.Title);
            
            return recipeMapper.MapToDto(recipe);
        }

        public async Task<RecipeDto> ImportFromMultipleImagesAsync(List<IFormFile> images, string processingMode = "sequential") {
            logger.LogInformation("Starting multi-image import with {ImageCount} images", images.Count);
            
            // Convert IFormFile streams to List<Stream>
            var streams = new List<Stream>();
            foreach (var image in images) {
                var stream = new MemoryStream();
                await image.CopyToAsync(stream).ConfigureAwait(false);
                stream.Position = 0; // Reset stream position
                streams.Add(stream);
            }

            try {
                var recipe = await importService.ImportFromMultipleImagesAsync(streams, processingMode).ConfigureAwait(false);
                
                // Save the imported recipe
                await uow.SaveChangesAsync().ConfigureAwait(false);
                
                logger.LogInformation("Multi-image import completed: {Title}", recipe.Title);
                
                return recipeMapper.MapToDto(recipe);
            }
            finally {
                // Clean up streams
                foreach (var stream in streams) {
                    await stream.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<RecipeDto> ImportStructuredAsync(ImportStructuredRequestDto dto) {
            logger.LogInformation("Starting structured import: {Title}", dto.Title);

            var recipe = await importService.ImportStructuredAsync(dto).ConfigureAwait(false);

            await uow.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Structured import completed: {Title}", recipe.Title);

            return recipeMapper.MapToDto(recipe);
        }

        public async Task<RecipeDto> ImportHtmlAsync(ImportHtmlRequestDto dto) {
            logger.LogInformation("Starting HTML import from: {Source}", dto.Source);

            var recipe = await importService.ImportHtmlAsync(dto).ConfigureAwait(false);

            await uow.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("HTML import completed: {Title}", recipe.Title);

            return recipeMapper.MapToDto(recipe);
        }
    }
}
