using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Input {
    public class IngredientSearchRequestDto {
        public List<string> Ingredients { get; set; } = new();
        public int MaxMissingIngredients { get; set; } = 5;
        public bool IncludePantryStaples { get; set; } = true;
        public bool IncludeSubstitutions { get; set; }
        public int? MaxCookTimeMinutes { get; set; }
        public Guid? DietaryProfileResourceId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortBy { get; set; } = "matchPercentage";
    }
}
