using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class AddRecipeToCollectionDto {
        [Required]
        public Guid RecipeResourceId { get; set; }

        public int? SortOrder { get; set; }
    }
}
