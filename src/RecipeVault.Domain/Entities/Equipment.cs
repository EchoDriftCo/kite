using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(Code), IsUnique = true)]
    [Table("Equipment")]
    public class Equipment : AuditableEntity {
        protected Equipment() {
        }

        public Equipment(string name, string code, EquipmentCategory category, string description = null, bool isCommon = false) {
            Update(name, code, category, description, isCommon);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EquipmentId { get; private set; }

        [Required]
        [StringLength(100)]
        public string Name { get; private set; }

        [Required]
        [StringLength(100)]
        public string Code { get; private set; }

        public EquipmentCategory Category { get; private set; }

        [StringLength(500)]
        public string Description { get; private set; }

        public bool IsCommon { get; private set; }

        private readonly List<UserEquipment> userEquipment = new();
        public virtual IReadOnlyList<UserEquipment> UserEquipment => userEquipment;

        public void Update(string name, string code, EquipmentCategory category, string description, bool isCommon) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(name), () => new InvalidValueError(nameof(name), name));
            messages.Aggregate(() => string.IsNullOrWhiteSpace(code), () => new InvalidValueError(nameof(code), code));
            messages.ThrowIfAny<ValidationListException>();

            Name = name;
            Code = code;
            Category = category;
            Description = description;
            IsCommon = isCommon;
        }
    }

    public enum EquipmentCategory {
        Appliance = 1,
        Cookware = 2,
        Bakeware = 3,
        Tool = 4
    }
}
