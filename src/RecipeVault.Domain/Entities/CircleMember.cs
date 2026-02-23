using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(CircleId), nameof(SubjectId), IsUnique = true)]
    [Table("CircleMember")]
    public class CircleMember {
        protected CircleMember() {
        }

        public CircleMember(int circleId, Guid subjectId, CircleRole role, MemberStatus status) {
            CircleId = circleId;
            SubjectId = subjectId;
            Role = role;
            Status = status;
            JoinedDate = status == MemberStatus.Active ? DateTime.UtcNow : (DateTime?)null;
            InvitedDate = status == MemberStatus.Pending ? DateTime.UtcNow : (DateTime?)null;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleMemberId { get; private set; }

        public int CircleId { get; private set; }
        public virtual Circle Circle { get; private set; }

        /// <summary>
        /// References Subject.SubjectId (Guid) - the user's resource identifier.
        /// </summary>
        public Guid SubjectId { get; private set; }
        public virtual Subject Subject { get; private set; }

        public CircleRole Role { get; private set; }

        public MemberStatus Status { get; private set; }

        public DateTime? JoinedDate { get; private set; }

        public DateTime? InvitedDate { get; private set; }

        public void UpdateRole(CircleRole newRole) {
            Role = newRole;
        }

        public void Activate() {
            Status = MemberStatus.Active;
            JoinedDate = DateTime.UtcNow;
        }

        public void Leave() {
            Status = MemberStatus.Left;
        }
    }
}
