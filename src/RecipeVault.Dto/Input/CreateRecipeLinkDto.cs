using System;

namespace RecipeVault.Dto.Input {
    public class CreateRecipeLinkDto {
        public Guid LinkedRecipeResourceId { get; set; }
        public int? IngredientIndex { get; set; }
        public string DisplayText { get; set; }
        public bool IncludeInTotalTime { get; set; }
        public decimal? PortionUsed { get; set; }
    }
}
