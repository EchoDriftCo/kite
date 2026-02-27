using System;

namespace RecipeVault.Dto.Input {
    /// <summary>
    /// Request to mix two recipes together
    /// </summary>
    public class MixRecipesRequestDto {
        /// <summary>
        /// First recipe to mix
        /// </summary>
        public Guid RecipeAId { get; set; }

        /// <summary>
        /// Second recipe to mix
        /// </summary>
        public Guid RecipeBId { get; set; }

        /// <summary>
        /// User's mixing intent (for guided mode)
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// Mixing mode: guided, surprise, bestOfBoth
        /// </summary>
        public string Mode { get; set; }
    }
}
