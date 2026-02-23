using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class ImportUrlRequestDto {
        [Required]
        [Url]
        [StringLength(1000)]
        public string Url { get; set; }
    }
}
