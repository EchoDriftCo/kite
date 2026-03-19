using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class AddAvoidedIngredientDto {
        [Required]
        [StringLength(100)]
        public string IngredientName { get; set; }

        [StringLength(200)]
        public string Reason { get; set; }
    }
}
