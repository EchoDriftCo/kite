using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IImportService {
        Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream);

        /// <summary>
        /// Import a recipe from a URL by fetching and parsing the page
        /// </summary>
        Task<Recipe> ImportFromUrlAsync(string url);

        /// <summary>
        /// Import a recipe from multiple images (e.g., multi-page cookbook photos)
        /// </summary>
        Task<Recipe> ImportFromMultipleImagesAsync(List<Stream> imageStreams, string processingMode = "sequential");

        /// <summary>
        /// Import a recipe from structured data (browser extension)
        /// </summary>
        Task<Recipe> ImportStructuredAsync(ImportStructuredRequestDto dto);

        /// <summary>
        /// Import a recipe from raw HTML content (browser extension server-side extraction)
        /// </summary>
        Task<Recipe> ImportHtmlAsync(ImportHtmlRequestDto dto);
    }
}
