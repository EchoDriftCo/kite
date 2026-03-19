using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class LinkedRecipeDto {
        public Guid RecipeLinkResourceId { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public int Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public int? TotalTimeMinutes { get; set; }
        public string OriginalImageUrl { get; set; }
        public int? IngredientIndex { get; set; }
        public string DisplayText { get; set; }
        public bool IncludeInTotalTime { get; set; }
        public decimal? PortionUsed { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; }
        public List<RecipeInstructionDto> Instructions { get; set; }
        public List<RecipeTagDto> Tags { get; set; }
    }
}
