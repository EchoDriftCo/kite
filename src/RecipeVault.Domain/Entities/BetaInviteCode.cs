using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(BetaInviteCodeResourceId), IsUnique = true)]
    [Index(nameof(Code), IsUnique = true)]
    [Table("BetaInviteCode")]
    public class BetaInviteCode : AuditableEntity {
        protected BetaInviteCode() {
        }

        public BetaInviteCode(string code, int maxUses, DateTime? expiresDate) {
            BetaInviteCodeResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            redemptions = new List<BetaInviteCodeRedemption>();
            IsActive = true;
            UseCount = 0;
            Update(code, maxUses, expiresDate);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BetaInviteCodeId { get; private set; }

        public Guid BetaInviteCodeResourceId { get; private set; }

        [Required]
        [StringLength(20)]
        public string Code { get; private set; }

        public int MaxUses { get; private set; }

        public int UseCount { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime? ExpiresDate { get; private set; }

        private readonly List<BetaInviteCodeRedemption> redemptions = new();
        public virtual IReadOnlyList<BetaInviteCodeRedemption> Redemptions => redemptions;

        public void Update(string code, int maxUses, DateTime? expiresDate) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(code), () => new InvalidValueError(nameof(code), code));
            messages.Aggregate(() => code?.Length > 20, () => new InvalidValueError(nameof(code), "Code must be 20 characters or fewer"));
            messages.Aggregate(() => maxUses < 1, () => new InvalidValueError(nameof(maxUses), "MaxUses must be at least 1"));
            messages.ThrowIfAny<ValidationListException>();

            Code = code;
            MaxUses = maxUses;
            ExpiresDate = expiresDate;
        }

        public void Deactivate() {
            IsActive = false;
        }

        public void Activate() {
            IsActive = true;
        }

        public void IncrementUseCount() {
            UseCount++;
        }

        public bool IsExpired() {
            return ExpiresDate.HasValue && ExpiresDate.Value <= DateTime.UtcNow;
        }

        public bool HasUsesRemaining() {
            return UseCount < MaxUses;
        }

        public bool IsValid() {
            return IsActive && !IsExpired() && HasUsesRemaining();
        }

        public void AddRedemption(BetaInviteCodeRedemption redemption) {
            redemptions.Add(redemption);
        }
    }
}
