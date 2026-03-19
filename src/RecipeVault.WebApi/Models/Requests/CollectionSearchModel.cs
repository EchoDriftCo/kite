namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Search model for collections
    /// </summary>
    public class CollectionSearchModel {
        /// <summary>
        /// Filter by public visibility
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Filter by featured status
        /// </summary>
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// Search term for name or description
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Page number
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Sort expression
        /// </summary>
        public string Sort { get; set; }
    }
}
