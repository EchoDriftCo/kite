using RecipeVault.Dto.Output;

namespace RecipeVault.Dto.Input {
    /// <summary>
    /// Request DTO for refining a generated recipe
    /// </summary>
    public class RefineRecipeRequestDto {
        /// <summary>
        /// The previously generated recipe to refine
        /// </summary>
        public GeneratedRecipeDto PreviousRecipe { get; set; }

        /// <summary>
        /// Refinement instructions (required)
        /// e.g., "make it spicier", "use chicken instead of beef"
        /// </summary>
        public string Refinement { get; set; }
    }
}
