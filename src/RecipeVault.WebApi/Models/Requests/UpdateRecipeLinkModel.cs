namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Request model for updating a recipe link
    /// </summary>
    public class UpdateRecipeLinkModel {
        /// <summary>
        /// Optional ingredient index where this link should appear
        /// </summary>
        public int? IngredientIndex { get; set; }

        /// <summary>
        /// Display text for the link (e.g., "My homemade pasta sauce")
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Whether to include the linked recipe's time in total time calculation
        /// </summary>
        public bool IncludeInTotalTime { get; set; }

        /// <summary>
        /// Portion of the linked recipe to use (e.g., 0.5 for half)
        /// </summary>
        public decimal? PortionUsed { get; set; }
    }
}
