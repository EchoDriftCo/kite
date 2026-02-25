using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    /// <summary>
    /// Collection response model
    /// </summary>
    public class CollectionModel {
        /// <summary>
        /// Collection resource ID
        /// </summary>
        public Guid CollectionResourceId { get; set; }

        /// <summary>
        /// Name of the collection
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the collection
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL to cover image
        /// </summary>
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Whether the collection is publicly visible
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Whether the collection is featured
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Number of recipes in the collection
        /// </summary>
        public int RecipeCount { get; set; }

        /// <summary>
        /// Date the collection was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date the collection was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Recipes in the collection
        /// </summary>
        public List<CollectionRecipeModel> Recipes { get; set; }
    }

    /// <summary>
    /// Recipe within a collection
    /// </summary>
    public class CollectionRecipeModel {
        /// <summary>
        /// Recipe resource ID
        /// </summary>
        public Guid RecipeResourceId { get; set; }

        /// <summary>
        /// Recipe title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Recipe description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Recipe image URL
        /// </summary>
        public string SourceImageUrl { get; set; }

        /// <summary>
        /// Sort order within the collection
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Date the recipe was added to the collection
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Total time in minutes
        /// </summary>
        public int? TotalTimeMinutes { get; set; }

        /// <summary>
        /// Recipe rating
        /// </summary>
        public int? Rating { get; set; }
    }
}
