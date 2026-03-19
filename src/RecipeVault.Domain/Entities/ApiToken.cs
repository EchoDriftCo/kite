using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(ApiTokenResourceId), IsUnique = true)]
    [Index(nameof(TokenHash), IsUnique = true)]
    [Index(nameof(SubjectId))]
    [Table("ApiToken")]
    public class ApiToken : AuditableEntity {
        protected ApiToken() {
        }

        public ApiToken(Guid apiTokenResourceId, Guid subjectId, string name, string tokenHash, string tokenPrefix, DateTime? expiresDate) {
            ApiTokenResourceId = apiTokenResourceId;
            SubjectId = subjectId;
            Name = name;
            TokenHash = tokenHash;
            TokenPrefix = tokenPrefix;
            ExpiresDate = expiresDate;
            IsRevoked = false;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApiTokenId { get; private set; }

        public Guid ApiTokenResourceId { get; private set; }

        public Guid SubjectId { get; private set; }

        [Required]
        [StringLength(100)]
        public string Name { get; private set; }

        [Required]
        [StringLength(64)]
        public string TokenHash { get; private set; }

        [Required]
        [StringLength(20)]
        public string TokenPrefix { get; private set; }

        public DateTime? LastUsedDate { get; private set; }

        public DateTime? ExpiresDate { get; private set; }

        public bool IsRevoked { get; private set; }

        public void MarkUsed() {
            LastUsedDate = DateTime.UtcNow;
        }

        public void Revoke() {
            IsRevoked = true;
        }

        public bool IsValid() {
            return !IsRevoked && (!ExpiresDate.HasValue || ExpiresDate > DateTime.UtcNow);
        }
    }
}
