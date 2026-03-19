using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class RecipeSummaryDto {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public int? TotalTimeMinutes { get; set; }
        public string OriginalImageUrl { get; set; }
        public int? Rating { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsOwner { get; set; }
        public List<RecipeTagDto> Tags { get; set; } = new();
        public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    }
}
