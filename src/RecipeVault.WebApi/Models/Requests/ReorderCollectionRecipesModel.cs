using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Model for reordering recipes within a collection
    /// </summary>
    public class ReorderCollectionRecipesModel {
        /// <summary>
        /// List of recipes with their new sort orders
        /// </summary>
        [Required]
        public List<RecipeOrderModel> Recipes { get; set; }
    }

    /// <summary>
    /// Model representing a recipe's sort order
    /// </summary>
    public class RecipeOrderModel {
        /// <summary>
        /// Resource ID of the recipe
        /// </summary>
        [Required]
        public Guid RecipeResourceId { get; set; }

        /// <summary>
        /// New sort order
        /// </summary>
        [Required]
        public int SortOrder { get; set; }
    }
}
