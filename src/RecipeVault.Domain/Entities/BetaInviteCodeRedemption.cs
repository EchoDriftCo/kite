using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeVault.Domain.Entities {
    [Table("BetaInviteCodeRedemption")]
    public class BetaInviteCodeRedemption {
        protected BetaInviteCodeRedemption() {
        }

        public BetaInviteCodeRedemption(int betaInviteCodeId, Guid subjectId) {
            BetaInviteCodeId = betaInviteCodeId;
            SubjectId = subjectId;
            RedeemedDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BetaInviteCodeRedemptionId { get; private set; }

        public int BetaInviteCodeId { get; private set; }

        public Guid SubjectId { get; private set; }

        public DateTime RedeemedDate { get; private set; }

        [ForeignKey(nameof(BetaInviteCodeId))]
        public virtual BetaInviteCode BetaInviteCode { get; private set; }
    }
}
