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
    [Index(nameof(DietaryProfileResourceId), IsUnique = true)]
    [Table("DietaryProfile")]
    public class DietaryProfile : AuditableEntity {
        protected DietaryProfile() {
        }

        public DietaryProfile(Guid subjectId, string profileName, bool isDefault = false) {
            DietaryProfileResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            SubjectId = subjectId;
            restrictions = new List<DietaryRestriction>();
            avoidedIngredients = new List<AvoidedIngredient>();
            Update(profileName, isDefault);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DietaryProfileId { get; private set; }

        public Guid DietaryProfileResourceId { get; private set; }

        public Guid SubjectId { get; private set; }

        [StringLength(100)]
        public string ProfileName { get; private set; }

        public bool IsDefault { get; private set; }

        private readonly List<DietaryRestriction> restrictions = new();
        public virtual IReadOnlyList<DietaryRestriction> Restrictions => restrictions;

        private readonly List<AvoidedIngredient> avoidedIngredients = new();
        public virtual IReadOnlyList<AvoidedIngredient> AvoidedIngredients => avoidedIngredients;

        public void Update(string profileName, bool isDefault) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(profileName), () => new InvalidValueError(nameof(profileName), profileName));
            messages.ThrowIfAny<ValidationListException>();

            ProfileName = profileName;
            IsDefault = isDefault;
        }

        public DietaryRestriction AddRestriction(string restrictionCode, string restrictionType, string severity) {
            // Check if already exists
            if (restrictions.Exists(r => r.RestrictionCode == restrictionCode)) {
                throw new InvalidOperationException($"Restriction '{restrictionCode}' already exists in this profile");
            }

            var restriction = new DietaryRestriction(DietaryProfileId, restrictionCode, restrictionType, severity);
            restrictions.Add(restriction);
            return restriction;
        }

        public void RemoveRestriction(DietaryRestriction restriction) {
            restrictions.Remove(restriction);
        }

        public AvoidedIngredient AddAvoidedIngredient(string ingredientName, string reason = null) {
            // Check if already exists
            if (avoidedIngredients.Exists(ai => ai.IngredientName.Equals(ingredientName, StringComparison.OrdinalIgnoreCase))) {
                throw new InvalidOperationException($"Ingredient '{ingredientName}' is already avoided in this profile");
            }

            var avoidedIngredient = new AvoidedIngredient(DietaryProfileId, ingredientName, reason);
            avoidedIngredients.Add(avoidedIngredient);
            return avoidedIngredient;
        }

        public void RemoveAvoidedIngredient(AvoidedIngredient avoidedIngredient) {
            avoidedIngredients.Remove(avoidedIngredient);
        }
    }
}
