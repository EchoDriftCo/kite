using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
            RawText = string.IsNullOrWhiteSpace(rawText)
                ? BuildRawText(quantity, unit, item, preparation)
                : rawText;
        }

        private static string BuildRawText(decimal? quantity, string unit, string item, string preparation) {
            var parts = new[] {
                quantity?.ToString(CultureInfo.InvariantCulture),
                unit,
                item,
                string.IsNullOrWhiteSpace(preparation) ? null : $"({preparation})"
            };
            return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
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

        public int? CanonicalUnitId { get; private set; }

        [ForeignKey(nameof(CanonicalUnitId))]
        public virtual Unit CanonicalUnit { get; private set; }

        public void SetCanonicalUnit(Unit unit) {
            CanonicalUnit = unit;
            CanonicalUnitId = unit?.UnitId;
        }
    }
}
