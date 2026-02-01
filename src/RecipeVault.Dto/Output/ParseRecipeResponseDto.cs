using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class ParseRecipeResponseDto {
        public double Confidence { get; set; }
        public ParsedRecipeDto Parsed { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class ParsedRecipeDto {
        public string Title { get; set; }
        public int? Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public List<ParsedIngredientDto> Ingredients { get; set; }
        public List<ParsedInstructionDto> Instructions { get; set; }
    }

    public class ParsedIngredientDto {
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
    }

    public class ParsedInstructionDto {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
    }
}
