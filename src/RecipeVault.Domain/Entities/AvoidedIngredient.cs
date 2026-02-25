using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(DietaryProfileId), nameof(IngredientName))]
    [Table("AvoidedIngredient")]
    public class AvoidedIngredient : AuditableEntity {
        protected AvoidedIngredient() {
        }

        public AvoidedIngredient(int dietaryProfileId, string ingredientName, string reason = null) {
            DietaryProfileId = dietaryProfileId;
            Update(ingredientName, reason);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AvoidedIngredientId { get; private set; }

        public int DietaryProfileId { get; private set; }
        public virtual DietaryProfile DietaryProfile { get; private set; }

        [Required]
        [StringLength(100)]
        public string IngredientName { get; private set; }

        [StringLength(200)]
        public string Reason { get; private set; }

        public void Update(string ingredientName, string reason) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(ingredientName), () => new InvalidValueError(nameof(ingredientName), ingredientName));
            messages.ThrowIfAny<ValidationListException>();

            IngredientName = ingredientName;
            Reason = reason;
        }
    }
}
