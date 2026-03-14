using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class IngredientSearchResultDto {
        public RecipeSummaryDto Recipe { get; set; }
        public List<string> MatchedIngredients { get; set; } = new();
        public List<string> MissingIngredients { get; set; } = new();
        public List<string> PantryStaplesUsed { get; set; } = new();
        public double MatchPercentage { get; set; }
        public double WeightedMatchPercentage { get; set; }
        public List<SubstitutionSuggestionDto> SubstitutionsAvailable { get; set; } = new();
    }
}
