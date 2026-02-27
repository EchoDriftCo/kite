using RecipeVault.Dto.Output;

namespace RecipeVault.Dto.Input {
    /// <summary>
    /// Request to refine a mixed recipe preview
    /// </summary>
    public class RefineMixRequestDto {
        /// <summary>
        /// The current mixed recipe preview to refine
        /// </summary>
        public MixedRecipePreviewDto Preview { get; set; }

        /// <summary>
        /// User's refinement notes
        /// </summary>
        public string RefinementNotes { get; set; }
    }
}
