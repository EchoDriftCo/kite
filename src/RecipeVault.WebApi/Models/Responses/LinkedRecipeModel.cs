using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    /// <summary>
    /// Response model for a linked recipe (component recipe)
    /// </summary>
    public class LinkedRecipeModel {
        /// <summary>
        /// The resource id of the link
        /// </summary>
        public Guid RecipeLinkResourceId { get; set; }

        /// <summary>
        /// The resource id of the linked recipe
        /// </summary>
        public Guid RecipeResourceId { get; set; }

        /// <summary>
        /// Recipe title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Yield (servings)
        /// </summary>
        public int Yield { get; set; }

        /// <summary>
        /// Prep time in minutes
        /// </summary>
        public int? PrepTimeMinutes { get; set; }

        /// <summary>
        /// Cook time in minutes
        /// </summary>
        public int? CookTimeMinutes { get; set; }

        /// <summary>
        /// Total time in minutes
        /// </summary>
        public int? TotalTimeMinutes { get; set; }

        /// <summary>
        /// Original image URL
        /// </summary>
        public string OriginalImageUrl { get; set; }

        /// <summary>
        /// Ingredient index where this link appears
        /// </summary>
        public int? IngredientIndex { get; set; }

        /// <summary>
        /// Display text for the link
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// Whether to include time in total
        /// </summary>
        public bool IncludeInTotalTime { get; set; }

        /// <summary>
        /// Portion of the recipe to use
        /// </summary>
        public decimal? PortionUsed { get; set; }

        /// <summary>
        /// Recipe ingredients
        /// </summary>
        public List<RecipeIngredientModel> Ingredients { get; set; }

        /// <summary>
        /// Recipe instructions
        /// </summary>
        public List<RecipeInstructionModel> Instructions { get; set; }

        /// <summary>
        /// Recipe tags
        /// </summary>
        public List<RecipeTagModel> Tags { get; set; }
    }
}
