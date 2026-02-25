using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(DietaryProfileId), nameof(RestrictionCode), IsUnique = true)]
    [Table("DietaryRestriction")]
    public class DietaryRestriction : AuditableEntity {
        protected DietaryRestriction() {
        }

        public DietaryRestriction(int dietaryProfileId, string restrictionCode, string restrictionType, string severity) {
            DietaryProfileId = dietaryProfileId;
            Update(restrictionCode, restrictionType, severity);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DietaryRestrictionId { get; private set; }

        public int DietaryProfileId { get; private set; }
        public virtual DietaryProfile DietaryProfile { get; private set; }

        [Required]
        [StringLength(50)]
        public string RestrictionType { get; private set; } // Allergy, Intolerance, DietaryChoice

        [Required]
        [StringLength(50)]
        public string RestrictionCode { get; private set; } // "peanuts", "dairy", "vegan", etc.

        [Required]
        [StringLength(20)]
        public string Severity { get; private set; } // Strict, Flexible

        public void Update(string restrictionCode, string restrictionType, string severity) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(restrictionCode), () => new InvalidValueError(nameof(restrictionCode), restrictionCode));
            messages.Aggregate(() => string.IsNullOrWhiteSpace(restrictionType), () => new InvalidValueError(nameof(restrictionType), restrictionType));
            messages.Aggregate(() => string.IsNullOrWhiteSpace(severity), () => new InvalidValueError(nameof(severity), severity));
            messages.ThrowIfAny<ValidationListException>();

            RestrictionCode = restrictionCode;
            RestrictionType = restrictionType;
            Severity = severity;
        }
    }
}
