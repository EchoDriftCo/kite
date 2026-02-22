using System.IO;
using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IImportFacade {
        Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream);
    }
}
