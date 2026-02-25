using System;
using System.Threading.Tasks;

namespace RecipeVault.DomainService {
    public interface IExportService {
        /// <summary>
        /// Export a recipe as JSON string
        /// </summary>
        Task<string> ExportRecipeAsJsonAsync(Guid recipeResourceId);

        /// <summary>
        /// Export a recipe as formatted plain text
        /// </summary>
        Task<string> ExportRecipeAsTextAsync(Guid recipeResourceId);

        /// <summary>
        /// Export a single recipe in Paprika format (gzipped JSON)
        /// </summary>
        Task<byte[]> ExportRecipeAsPaprikaAsync(Guid recipeResourceId);

        /// <summary>
        /// Export all user recipes as .paprikarecipes file (gzipped JSON array)
        /// </summary>
        Task<byte[]> ExportAllAsPaprikaAsync(Guid subjectId);
    }
}
