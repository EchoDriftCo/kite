using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;

namespace RecipeVault.Domain.Entities {
    [Table("RecipeIngredient")]
    public class RecipeIngredient : AuditableEntity {
        protected RecipeIngredient() {
        }

        public RecipeIngredient(int sortOrder, decimal? quantity, string unit, string item, string preparation, string rawText) {
            SortOrder = sortOrder;
            Quantity = quantity;
            Unit = unit;
            Item = item;
            Preparation = preparation;
            RawText = rawText;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeIngredientId { get; private set; }

        [ForeignKey(nameof(RecipeId))]
        public int RecipeId { get; private set; }

        public int SortOrder { get; private set; }

        [Column(TypeName = "numeric(10,3)")]
        public decimal? Quantity { get; private set; }

        [StringLength(20)]
        public string Unit { get; private set; }

        [Required]
        [StringLength(250)]
        public string Item { get; private set; }

        [StringLength(250)]
        public string Preparation { get; private set; }

        [Required]
        public string RawText { get; private set; }
    }
}
