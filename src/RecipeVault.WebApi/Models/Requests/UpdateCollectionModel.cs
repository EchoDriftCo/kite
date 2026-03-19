using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Model for creating or updating a collection
    /// </summary>
    public class UpdateCollectionModel {
        /// <summary>
        /// Name of the collection
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the collection
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// URL to cover image
        /// </summary>
        [StringLength(1000)]
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Whether the collection is publicly visible
        /// </summary>
        public bool? IsPublic { get; set; }
    }
}
