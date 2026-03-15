using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class UpdatePantryItemDto {
        [Required]
        [MaxLength(250)]
        public string IngredientName { get; set; }

        public bool IsStaple { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
