using System.Collections.Generic;

namespace RecipeVault.DomainService.Models {
    /// <summary>
    /// Preview of a mixed recipe with attribution notes
    /// </summary>
    public class MixedRecipePreview {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public List<MixedIngredient> Ingredients { get; set; } = new();
        public List<MixedInstruction> Instructions { get; set; } = new();
        public string MixNotes { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Ingredient with attribution (from A, B, or combined)
    /// </summary>
    public class MixedIngredient {
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
        public string Attribution { get; set; } // "from A", "from B", "combined"
    }

    /// <summary>
    /// Instruction with attribution
    /// </summary>
    public class MixedInstruction {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
        public string Attribution { get; set; } // "from A", "from B", "combined"
    }
}
