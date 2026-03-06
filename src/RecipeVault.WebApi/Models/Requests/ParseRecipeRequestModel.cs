namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Request model for parsing a recipe image
    /// </summary>
    public class ParseRecipeRequestModel {
        /// <summary>
        /// Base64 encoded image data
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// MIME type of the image (e.g., "image/jpeg", "image/png")
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// URL of a recipe webpage to parse
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Raw HTML content from a recipe page (browser extension fallback)
        /// </summary>
        public string Html { get; set; }
    }
}
