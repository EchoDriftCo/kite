using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class UpdateDietaryProfileDto {
        [Required]
        [StringLength(100)]
        public string ProfileName { get; set; }

        public bool IsDefault { get; set; }
    }
}
