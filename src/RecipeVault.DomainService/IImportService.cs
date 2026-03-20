using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IImportService {
        Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream);

        /// <summary>
        /// Import a recipe from a URL by fetching and parsing the page
        /// </summary>
        Task<Recipe> ImportFromUrlAsync(string url);

        /// <summary>
        /// Import a recipe from multiple images (e.g., multi-page cookbook photos)
        /// </summary>
        Task<Recipe> ImportFromMultipleImagesAsync(List<Stream> imageStreams, string processingMode = "sequential");

        /// <summary>
        /// Import a recipe from structured data (browser extension)
        /// </summary>
        Task<Recipe> ImportStructuredAsync(ImportStructuredRequestDto dto);

        /// <summary>
        /// Import a recipe from raw HTML content (browser extension server-side extraction)
        /// </summary>
        Task<Recipe> ImportHtmlAsync(ImportHtmlRequestDto dto);

        /// <summary>
        /// Import a recipe from a video URL (TikTok, Instagram, YouTube)
        /// </summary>
        Task<VideoImportResult> ImportFromVideoAsync(string videoUrl, bool includeSubtitles = true);
    }

    /// <summary>
    /// Result from video import operation
    /// </summary>
    public class VideoImportResult {
        /// <summary>
        /// Imported recipe entity
        /// </summary>
        public Recipe Recipe { get; set; }

        /// <summary>
        /// Transcribed audio text
        /// </summary>
        public string Transcript { get; set; }

        /// <summary>
        /// Transcript confidence score
        /// </summary>
        public decimal TranscriptConfidence { get; set; }

        /// <summary>
        /// Video platform name
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Video duration (formatted string)
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Video thumbnail URL
        /// </summary>
        public string ThumbnailUrl { get; set; }
    }
}
