using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class RedeemInviteCodeDto {
        [Required]
        [StringLength(20)]
        public string Code { get; set; }
    }
}
