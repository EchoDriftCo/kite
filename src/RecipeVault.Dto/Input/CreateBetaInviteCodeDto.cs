using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class CreateBetaInviteCodeDto {
        [Range(1, 100)]
        public int Count { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int MaxUses { get; set; } = 1;

        public DateTime? ExpiresDate { get; set; }
    }
}
