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

        public async Task<string> ImportFromUrlAsync(string sourceUrl, string fileName) {
            if (string.IsNullOrWhiteSpace(sourceUrl)) {
                return null;
            }

            try {
                logger.LogInformation("Downloading image from {Url}", sourceUrl);

                // Download the image
                using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
                // Set a user agent to avoid being blocked
                downloadRequest.Headers.UserAgent.ParseAdd("RecipeVault/1.0");
                
                var downloadResponse = await httpClient.SendAsync(downloadRequest).ConfigureAwait(false);
                
                if (!downloadResponse.IsSuccessStatusCode) {
                    logger.LogWarning("Failed to download image from {Url}: {StatusCode}", sourceUrl, downloadResponse.StatusCode);
                    return null;
                }

                var imageData = await downloadResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var contentType = downloadResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

                // Validate it's actually an image
                if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) {
                    logger.LogWarning("Downloaded content is not an image: {ContentType}", contentType);
                    return null;
                }

                // Upload to our storage
                var uploadedUrl = await UploadAsync(imageData, fileName, contentType).ConfigureAwait(false);
                logger.LogInformation("Successfully imported image from {SourceUrl} to {UploadedUrl}", sourceUrl, uploadedUrl);
                
                return uploadedUrl;
            } catch (Exception ex) {
                logger.LogWarning(ex, "Failed to import image from {Url}", sourceUrl);
                return null;
            }
        }
    }
}
