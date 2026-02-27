using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(SubjectId), nameof(EquipmentId), IsUnique = true)]
    [Table("UserEquipment")]
    public class UserEquipment : AuditableEntity {
        protected UserEquipment() {
        }

        public UserEquipment(Guid subjectId, int equipmentId) {
            var messages = new MessageList();
            messages.Aggregate(() => subjectId == Guid.Empty, () => new InvalidValueError(nameof(subjectId), subjectId.ToString()));
            messages.Aggregate(() => equipmentId <= 0, () => new InvalidValueError(nameof(equipmentId), equipmentId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            messages.ThrowIfAny<ValidationListException>();

            SubjectId = subjectId;
            EquipmentId = equipmentId;
            AddedDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserEquipmentId { get; private set; }

        public Guid SubjectId { get; private set; }

        public int EquipmentId { get; private set; }
        public virtual Equipment Equipment { get; private set; }

        public DateTime AddedDate { get; private set; }
    }
}
