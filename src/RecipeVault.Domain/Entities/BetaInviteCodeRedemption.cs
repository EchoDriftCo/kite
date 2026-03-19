using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(BetaInviteCodeId))]
    [Index(nameof(SubjectId))]
    [Table("BetaInviteCodeRedemption")]
    public class BetaInviteCodeRedemption {
        protected BetaInviteCodeRedemption() {
        }

        public BetaInviteCodeRedemption(int betaInviteCodeId, Guid subjectId, AccountTier previousTier, AccountTier newTier) {
            BetaInviteCodeId = betaInviteCodeId;
            SubjectId = subjectId;
            RedeemedDate = DateTime.UtcNow;
            PreviousTier = previousTier;
            NewTier = newTier;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BetaInviteCodeRedemptionId { get; private set; }

        public int BetaInviteCodeId { get; private set; }

        public Guid SubjectId { get; private set; }

        public DateTime RedeemedDate { get; private set; }

        public AccountTier PreviousTier { get; private set; }

        public AccountTier NewTier { get; private set; }

        [ForeignKey(nameof(BetaInviteCodeId))]
        public virtual BetaInviteCode BetaInviteCode { get; private set; }
    }
}
