using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(InviteToken), IsUnique = true)]
    [Index(nameof(CircleId))]
    [Table("CircleInvite")]
    public class CircleInvite {
        protected CircleInvite() {
        }

        public CircleInvite(int circleId, string inviteeEmail, int invitedBySubjectId, DateTime expiresDate) {
            InviteToken = Uuid.NewDatabaseFriendly(Database.SqlServer);
            CircleId = circleId;
            InviteeEmail = inviteeEmail;
            InvitedBySubjectId = invitedBySubjectId;
            CreatedDate = DateTime.UtcNow;
            ExpiresDate = expiresDate;
            Status = InviteStatus.Pending;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleInviteId { get; private set; }

        public Guid InviteToken { get; private set; }

        public int CircleId { get; private set; }
        public virtual Circle Circle { get; private set; }

        [StringLength(255)]
        public string InviteeEmail { get; private set; }

        public int InvitedBySubjectId { get; private set; }

        public DateTime CreatedDate { get; private set; }

        public DateTime ExpiresDate { get; private set; }

        public InviteStatus Status { get; private set; }

        public void Accept() {
            if (Status != InviteStatus.Pending) {
                throw new InvalidOperationException("Invite has already been processed");
            }
            if (DateTime.UtcNow > ExpiresDate) {
                Status = InviteStatus.Expired;
                throw new InvalidOperationException("Invite has expired");
            }
            Status = InviteStatus.Accepted;
        }

        public void Revoke() {
            if (Status == InviteStatus.Pending) {
                Status = InviteStatus.Revoked;
            }
        }

        public bool IsExpired() {
            return DateTime.UtcNow > ExpiresDate && Status == InviteStatus.Pending;
        }
    }
}
