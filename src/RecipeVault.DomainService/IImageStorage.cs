using System.Threading.Tasks;

namespace RecipeVault.DomainService {
    public interface IImageStorage {
        Task<string> UploadAsync(byte[] imageData, string fileName, string contentType);
        Task DeleteAsync(string path);
        
        /// <summary>
        /// Downloads an image from an external URL and uploads it to storage.
        /// Returns the public URL of the uploaded image, or null if download fails.
        /// </summary>
        Task<string> ImportFromUrlAsync(string sourceUrl, string fileName);
    }
}
