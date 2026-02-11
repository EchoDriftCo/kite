#pragma warning disable CS1591 // Missing XML comments

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class UpdateRecipeModel {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public int Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public string Source { get; set; }
        public string OriginalImageUrl { get; set; }
        public List<UpdateRecipeIngredientModel> Ingredients { get; set; }
        public List<UpdateRecipeInstructionModel> Instructions { get; set; }
        public bool IsPublic { get; set; }
    }

    public class UpdateRecipeIngredientModel {
        public int SortOrder { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        [Required]
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
    }

    public class UpdateRecipeInstructionModel {
        public int StepNumber { get; set; }
        [Required]
        public string Instruction { get; set; }
        public string RawText { get; set; }
    }
}
