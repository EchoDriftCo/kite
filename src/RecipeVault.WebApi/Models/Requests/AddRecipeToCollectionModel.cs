using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Model for adding a recipe to a collection
    /// </summary>
    public class AddRecipeToCollectionModel {
        /// <summary>
        /// Resource ID of the recipe to add
        /// </summary>
        [Required]
        public Guid RecipeResourceId { get; set; }

        /// <summary>
        /// Sort order within the collection (optional)
        /// </summary>
        public int? SortOrder { get; set; }
    }
}
