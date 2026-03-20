namespace RecipeVault.Integrations.VideoDownload {
    /// <summary>
    /// Result from video download operation
    /// </summary>
    public class VideoDownloadResult {
        /// <summary>
        /// Audio data in bytes
        /// </summary>
        public byte[] AudioData { get; set; }

        /// <summary>
        /// Audio format (e.g., "mp3", "m4a", "wav")
        /// </summary>
        public string AudioFormat { get; set; }

        /// <summary>
        /// Platform name (e.g., "youtube", "tiktok", "instagram")
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Video duration in seconds
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Thumbnail URL
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Extracted subtitles/captions text (if available)
        /// </summary>
        public string Subtitles { get; set; }

        /// <summary>
        /// Video title from metadata
        /// </summary>
        public string VideoTitle { get; set; }
    }
}
