using System;
using System.Runtime.Serialization;

namespace RecipeVault.Integrations.Gemini.Exceptions {
    /// <summary>
    /// Exception thrown when Gemini API returns an error or cannot parse a recipe
    /// </summary>
    [Serializable]
    public class GeminiApiException : Exception {
        /// <summary>
        /// HTTP status code from Gemini API response
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Raw error message from API
        /// </summary>
        public string ApiErrorMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of GeminiApiException
        /// </summary>
        public GeminiApiException() {
        }

        /// <summary>
        /// Initializes a new instance of GeminiApiException with message
        /// </summary>
        public GeminiApiException(string message) : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of GeminiApiException with message and status code
        /// </summary>
        public GeminiApiException(string message, int? statusCode = null, string apiErrorMessage = null)
            : base(message) {
            StatusCode = statusCode;
            ApiErrorMessage = apiErrorMessage;
        }

        /// <summary>
        /// Initializes a new instance of GeminiApiException with message and inner exception
        /// </summary>
        public GeminiApiException(string message, Exception innerException) : base(message, innerException) {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        protected GeminiApiException(SerializationInfo info, StreamingContext context) : base(info, context) {
            StatusCode = info.GetInt32("StatusCode");
            ApiErrorMessage = info.GetString("ApiErrorMessage");
        }
    }
}
