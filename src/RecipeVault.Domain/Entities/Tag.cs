using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(TagResourceId), IsUnique = true)]
    [Index(nameof(Name), nameof(Category), IsUnique = true)]
    [Table("Tag")]
    public class Tag : AuditableEntity {
        protected Tag() {
        }

        public Tag(string name, TagCategory category, bool isGlobal) {
            TagResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            IsGlobal = isGlobal;
            Update(name, category);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagId { get; private set; }

        public Guid TagResourceId { get; private set; }

        [Required]
        [StringLength(100)]
        public string Name { get; private set; }

        public TagCategory Category { get; private set; }

        public bool IsGlobal { get; private set; }

        public void Update(string name, TagCategory category) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(name), () => new InvalidValueError(nameof(name), name));
            messages.ThrowIfAny<ValidationListException>();

            Name = name;
            Category = category;
        }
    }
}
