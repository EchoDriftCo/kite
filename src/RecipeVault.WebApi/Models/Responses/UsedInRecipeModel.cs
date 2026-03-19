using System;

namespace RecipeVault.WebApi.Models.Responses {
    /// <summary>
    /// Response model for a recipe that uses this recipe as a component
    /// </summary>
    public class UsedInRecipeModel {
        /// <summary>
        /// The resource id of the link
        /// </summary>
        public Guid RecipeLinkResourceId { get; set; }

        /// <summary>
        /// The resource id of the parent recipe
        /// </summary>
        public Guid RecipeResourceId { get; set; }

        /// <summary>
        /// Recipe title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Recipe image URL
        /// </summary>
        public string OriginalImageUrl { get; set; }

        /// <summary>
        /// Owner's name
        /// </summary>
        public string OwnerName { get; set; }
    }
}
