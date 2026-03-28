using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(PremiumWaitlistResourceId), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Table("PremiumWaitlist")]
    public class PremiumWaitlist : AuditableEntity {
        protected PremiumWaitlist() { }

        public PremiumWaitlist(string email, string source) {
            PremiumWaitlistResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            Email = email?.Trim().ToLowerInvariant();
            Source = source ?? "features-page";
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PremiumWaitlistId { get; private set; }
        public Guid PremiumWaitlistResourceId { get; private set; }

        [Required]
        [StringLength(320)]
        public string Email { get; private set; }

        [StringLength(50)]
        public string Source { get; private set; }
    }
}
