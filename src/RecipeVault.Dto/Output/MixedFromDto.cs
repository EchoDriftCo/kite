using System;

namespace RecipeVault.Dto.Output {
    /// <summary>
    /// Information about the parent recipes that were mixed to create this recipe
    /// </summary>
    public class MixedFromDto {
        public Guid RecipeAResourceId { get; set; }
        public string RecipeATitle { get; set; }
        public Guid RecipeBResourceId { get; set; }
        public string RecipeBTitle { get; set; }
        public string MixIntent { get; set; }
    }
}
