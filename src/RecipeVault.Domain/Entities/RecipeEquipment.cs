using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(RecipeId), nameof(EquipmentId), IsUnique = true)]
    [Table("RecipeEquipment")]
    public class RecipeEquipment : AuditableEntity {
        protected RecipeEquipment() {
        }

        public RecipeEquipment(int recipeId, int equipmentId, bool isRequired = true) {
            var messages = new MessageList();
            messages.Aggregate(() => recipeId <= 0, () => new InvalidValueError(nameof(recipeId), recipeId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            messages.Aggregate(() => equipmentId <= 0, () => new InvalidValueError(nameof(equipmentId), equipmentId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            messages.ThrowIfAny<ValidationListException>();

            RecipeId = recipeId;
            EquipmentId = equipmentId;
            IsRequired = isRequired;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeEquipmentId { get; private set; }

        public int RecipeId { get; private set; }
        public virtual Recipe Recipe { get; private set; }

        public int EquipmentId { get; private set; }
        public virtual Equipment Equipment { get; private set; }

        public bool IsRequired { get; private set; }

        public void SetIsRequired(bool isRequired) {
            IsRequired = isRequired;
        }
    }
}
