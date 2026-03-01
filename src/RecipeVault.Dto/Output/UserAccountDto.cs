using System;

namespace RecipeVault.Dto.Output {
    public class UserAccountDto {
        public Guid UserAccountResourceId { get; set; }
        public Guid SubjectId { get; set; }
        public string AccountTier { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? TierChangedDate { get; set; }
    }
}
