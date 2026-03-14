using System.Collections.Generic;

namespace RecipeVault.Dto.Input {
    public class ImportStructuredRequestDto {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public string Source { get; set; }
        public string OriginalImageUrl { get; set; }
        public List<string> RawIngredients { get; set; }
        public List<string> RawInstructions { get; set; }
        public List<string> Categories { get; set; }
    }
}
