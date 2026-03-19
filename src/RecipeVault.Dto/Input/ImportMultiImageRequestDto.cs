namespace RecipeVault.Dto.Input {
    /// <summary>
    /// Request to import a recipe from multiple images (e.g., multi-page cookbook photos)
    /// </summary>
    public class ImportMultiImageRequestDto {
        /// <summary>
        /// Processing mode: "sequential" (process in order, concat results) or "stitch" (combine images first)
        /// </summary>
        public string ProcessingMode { get; set; } = "sequential";
    }
}
