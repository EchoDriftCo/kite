using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Request model for video import
    /// </summary>
    public class ImportVideoRequestModel {
        /// <summary>
        /// Video URL (TikTok, Instagram, YouTube, etc.)
        /// </summary>
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// Whether to extract subtitles/captions if available
        /// </summary>
        public bool IncludeSubtitles { get; set; } = true;
    }
}
