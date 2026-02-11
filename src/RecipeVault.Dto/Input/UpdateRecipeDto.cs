using System.Collections.Generic;

namespace RecipeVault.Dto.Input {
    public class UpdateRecipeDto {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public string Source { get; set; }
        public string OriginalImageUrl { get; set; }
        public List<UpdateRecipeIngredientDto> Ingredients { get; set; }
        public List<UpdateRecipeInstructionDto> Instructions { get; set; }
        public bool IsPublic { get; set; }
    }

    public class UpdateRecipeIngredientDto {
        public int SortOrder { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
    }

    public class UpdateRecipeInstructionDto {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
    }
}
