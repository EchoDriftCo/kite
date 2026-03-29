#pragma warning disable CS1591 // Missing XML comments

using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class UserAccountModel {
        public Guid UserAccountResourceId { get; set; }
        public Guid SubjectId { get; set; }
        public string AccountTier { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TierChangedDate { get; set; }
        public DateTime? BetaCodeRedeemedDate { get; set; }
    }
}
