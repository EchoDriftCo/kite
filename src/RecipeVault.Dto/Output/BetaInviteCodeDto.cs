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
        public List<BetaInviteCodeRedemptionDto> Redemptions { get; set; }
    }

    public class BetaInviteCodeRedemptionDto {
        public Guid SubjectId { get; set; }
        public DateTime RedeemedDate { get; set; }
    }

    public class ValidateInviteCodeResultDto {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
