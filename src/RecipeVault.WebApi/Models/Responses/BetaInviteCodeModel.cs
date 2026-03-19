#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class BetaInviteCodeModel {
        public Guid BetaInviteCodeResourceId { get; set; }
        public string Code { get; set; }
        public int MaxUses { get; set; }
        public int UseCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }

    public class RedeemCodeResultModel {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public UserAccountModel UpdatedAccount { get; set; }
    }

    public class ValidateInviteCodeResultModel {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
