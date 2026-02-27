using System;

namespace RecipeVault.WebApi.Models.Responses {
    /// <summary>
    /// Response model for a recipe link
    /// </summary>
    public class RecipeLinkModel {
        /// <summary>
        /// The resource id of the link
        /// </summary>
        public Guid RecipeLinkResourceId { get; set; }

        /// <summary>
        /// The internal id of the parent recipe
        /// </summary>
        public int ParentRecipeId { get; set; }

        /// <summary>
        /// The internal id of the linked recipe
        /// </summary>
        public int LinkedRecipeId { get; set; }

        /// <summary>
        /// Optional ingredient index where this link appears
        /// </summary>
        public int? IngredientIndex { get; set; }

        /// <summary>
        /// Display text for the link
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Whether to include the linked recipe's time in total time calculation
        /// </summary>
        public bool IncludeInTotalTime { get; set; }

        /// <summary>
        /// Portion of the linked recipe to use
        /// </summary>
        public decimal? PortionUsed { get; set; }
    }
}
