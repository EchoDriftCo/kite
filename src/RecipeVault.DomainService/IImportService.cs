using System.IO;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IImportService {
        Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream);
        
        /// <summary>
        /// Import a recipe from a URL by fetching and parsing the page
        /// </summary>
        /// <param name="url">The URL of the recipe page</param>
        /// <returns>The imported recipe</returns>
        Task<Recipe> ImportFromUrlAsync(string url);
    }
}
