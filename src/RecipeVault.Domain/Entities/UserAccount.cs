using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(UserAccountResourceId), IsUnique = true)]
    [Index(nameof(SubjectId), IsUnique = true)]
    [Table("UserAccount")]
    public class UserAccount : AuditableEntity {
        protected UserAccount() {
        }

        public UserAccount(Guid subjectId) {
            UserAccountResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            SubjectId = subjectId;
            AccountTier = AccountTier.Free;
            CreatedDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserAccountId { get; private set; }

        public Guid UserAccountResourceId { get; private set; }

        public Guid SubjectId { get; private set; }

        public AccountTier AccountTier { get; private set; }

        public DateTime? TierChangedDate { get; private set; }

        public void SetTier(AccountTier tier) {
            if (AccountTier != tier) {
                AccountTier = tier;
                TierChangedDate = DateTime.UtcNow;
            }
        }
    }
}
