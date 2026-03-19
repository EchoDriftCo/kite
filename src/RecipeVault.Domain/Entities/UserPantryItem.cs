using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(SubjectId), nameof(IngredientName), IsUnique = true)]
    [Index(nameof(SubjectId), nameof(IsStaple))]
    [Table("UserPantryItem")]
    public class UserPantryItem : AuditableEntity {
        protected UserPantryItem() {
        }

        public UserPantryItem(Guid subjectId, string ingredientName, bool isStaple = false, DateTime? expirationDate = null) {
            SubjectId = subjectId;
            IngredientName = ingredientName;
            IsStaple = isStaple;
            ExpirationDate = expirationDate;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserPantryItemId { get; private set; }

        public Guid SubjectId { get; private set; }

        [Required]
        [StringLength(250)]
        public string IngredientName { get; private set; }

        public bool IsStaple { get; private set; }

        public DateTime? ExpirationDate { get; private set; }

        public void Update(string ingredientName, bool isStaple, DateTime? expirationDate) {
            IngredientName = ingredientName;
            IsStaple = isStaple;
            ExpirationDate = expirationDate;
        }
    }
}
