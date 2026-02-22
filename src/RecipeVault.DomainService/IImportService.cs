using System.IO;
using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IImportService {
        Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream);
    }
}
