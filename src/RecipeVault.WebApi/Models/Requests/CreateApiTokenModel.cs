using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class CreateApiTokenModel {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public int? ExpiresInDays { get; set; }
    }
}
