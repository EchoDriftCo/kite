using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IImportFacade {
        Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream);
        Task<RecipeDto> ImportFromUrlAsync(string url);
        Task<RecipeDto> ImportFromMultipleImagesAsync(List<IFormFile> images, string processingMode = "sequential");
        Task<RecipeDto> ImportStructuredAsync(ImportStructuredRequestDto dto);
        Task<RecipeDto> ImportHtmlAsync(ImportHtmlRequestDto dto);
    }
}
