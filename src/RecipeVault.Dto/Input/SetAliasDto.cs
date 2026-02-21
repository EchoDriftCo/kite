using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class SetAliasDto {
        [Required]
        [StringLength(100)]
        public string Alias { get; set; }
        
        public bool ShowAliasPublicly { get; set; }
    }
}
