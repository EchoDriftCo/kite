using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class ReorderCollectionRecipesDto {
        [Required]
        public List<RecipeOrderDto> Recipes { get; set; }
    }

    public class RecipeOrderDto {
        [Required]
        public Guid RecipeResourceId { get; set; }

        [Required]
        public int SortOrder { get; set; }
    }
}
