using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeVault.Configuration;
using RecipeVault.DomainService;

namespace RecipeVault.Integrations.Supabase {
    public class SupabaseStorageClient : IImageStorage {
        private readonly HttpClient httpClient;
        private readonly SupabaseConfiguration config;
        private readonly ILogger<SupabaseStorageClient> logger;

        public SupabaseStorageClient(HttpClient httpClient, IOptions<SupabaseConfiguration> config, ILogger<SupabaseStorageClient> logger) {
            this.httpClient = httpClient;
            this.config = config.Value;
            this.logger = logger;
        }

        public async Task<string> UploadAsync(byte[] imageData, string fileName, string contentType) {
            var bucket = config.StorageBucket;
            var path = $"storage/v1/object/{bucket}/{fileName}";

            using var content = new ByteArrayContent(imageData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Content = content;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ServiceKey);

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                logger.LogError("Supabase Storage upload failed: {StatusCode} {Body}", response.StatusCode, body);
                throw new InvalidOperationException($"Image upload failed: {response.StatusCode}");
            }

            var publicUrl = $"{config.Url}/storage/v1/object/public/{bucket}/{fileName}";
            logger.LogInformation("Image uploaded to Supabase Storage: {Url}", publicUrl);
            return publicUrl;
        }

        public async Task DeleteAsync(string path) {
            var bucket = config.StorageBucket;
            var requestPath = $"storage/v1/object/{bucket}/{path}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, requestPath);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ServiceKey);

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                logger.LogWarning("Supabase Storage delete failed: {StatusCode} {Body}", response.StatusCode, body);
            }
        }
    }
}
