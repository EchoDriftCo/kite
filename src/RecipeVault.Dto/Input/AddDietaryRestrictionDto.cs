using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class AddDietaryRestrictionDto {
        [Required]
        [StringLength(50)]
        public string RestrictionCode { get; set; }

        [Required]
        [StringLength(50)]
        public string RestrictionType { get; set; } // Allergy, Intolerance, DietaryChoice

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } // Strict, Flexible
    }
}
