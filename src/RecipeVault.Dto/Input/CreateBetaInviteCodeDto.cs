using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class CreateBetaInviteCodeDto {
        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        public int MaxUses { get; set; } = 1;

        public DateTime? ExpiresDate { get; set; }
    }
}
