#pragma warning disable CS1591 // Missing XML comments

using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class ParseRecipeResponseModel {
        public double Confidence { get; set; }
        public ParsedRecipeModel Parsed { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class ParsedRecipeModel {
        public string Title { get; set; }
        public int? Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public List<ParsedIngredientModel> Ingredients { get; set; }
        public List<ParsedInstructionModel> Instructions { get; set; }
    }

    public class ParsedIngredientModel {
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
    }

    public class ParsedInstructionModel {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
    }
}
