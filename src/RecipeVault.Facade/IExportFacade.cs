using System;
using System.Threading.Tasks;

namespace RecipeVault.Facade {
    public interface IExportFacade {
        Task<string> ExportRecipeAsJsonAsync(Guid recipeResourceId);
        Task<string> ExportRecipeAsTextAsync(Guid recipeResourceId);
        Task<byte[]> ExportRecipeAsPaprikaAsync(Guid recipeResourceId);
        Task<byte[]> ExportAllAsPaprikaAsync();
    }
}
