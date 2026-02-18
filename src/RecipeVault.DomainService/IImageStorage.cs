using System.Threading.Tasks;

namespace RecipeVault.DomainService {
    public interface IImageStorage {
        Task<string> UploadAsync(byte[] imageData, string fileName, string contentType);
        Task DeleteAsync(string path);
    }
}
