using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    /// <summary>
    /// Preview of a mixed recipe with attribution notes
    /// </summary>
    public class MixedRecipePreviewDto {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public List<MixedIngredientDto> Ingredients { get; set; } = new();
        public List<MixedInstructionDto> Instructions { get; set; } = new();
        public string MixNotes { get; set; }
        public string Source { get; set; }
        
        // Parent recipe tracking
        public Guid? RecipeAResourceId { get; set; }
        public Guid? RecipeBResourceId { get; set; }
    }

    /// <summary>
    /// Ingredient with attribution
    /// </summary>
    public class MixedIngredientDto {
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
    public class MixedInstructionDto {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
        public string Attribution { get; set; } // "from A", "from B", "combined"
    }
}
