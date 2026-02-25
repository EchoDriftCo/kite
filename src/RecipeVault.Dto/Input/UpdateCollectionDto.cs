using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class UpdateCollectionDto {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(1000)]
        public string CoverImageUrl { get; set; }

        public bool? IsPublic { get; set; }
    }
}
