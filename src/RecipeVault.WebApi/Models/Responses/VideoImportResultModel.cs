namespace RecipeVault.WebApi.Models.Responses {
    /// <summary>
    /// Response model for video import
    /// </summary>
    public class VideoImportResultModel {
        /// <summary>
        /// Imported recipe
        /// </summary>
        public RecipeModel Recipe { get; set; }

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
