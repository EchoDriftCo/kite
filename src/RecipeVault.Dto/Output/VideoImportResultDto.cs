namespace RecipeVault.Dto.Output {
    /// <summary>
    /// Result from video import operation
    /// </summary>
    public class VideoImportResultDto {
        /// <summary>
        /// Imported recipe
        /// </summary>
        public RecipeDto Recipe { get; set; }

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
