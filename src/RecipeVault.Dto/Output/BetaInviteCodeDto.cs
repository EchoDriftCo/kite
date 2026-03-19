using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class BetaInviteCodeDto {
        public Guid BetaInviteCodeResourceId { get; set; }
        public string Code { get; set; }
        public int MaxUses { get; set; }
        public int UseCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }

    public class RedeemCodeResultDto {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public UserAccountDto UpdatedAccount { get; set; }
    }

    public class ValidateInviteCodeResultDto {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
