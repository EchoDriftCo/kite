using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class JoinWaitlistModel {
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Source { get; set; }
    }
}
