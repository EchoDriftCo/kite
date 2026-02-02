namespace RecipeVault.Configuration {
    /// <summary>
    /// Configuration for Google Gemini API integration
    /// </summary>
    public class GeminiConfiguration {
        /// <summary>
        /// API key for authentication with Gemini API
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Model identifier (e.g., "gemini-1.5-flash", "gemini-1.5-pro")
        /// </summary>
        public string Model { get; set; } = "gemini-1.5-flash";

        /// <summary>
        /// Base URL endpoint for Gemini API
        /// </summary>
        public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
    }
}
