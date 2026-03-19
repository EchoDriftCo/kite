using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;

namespace RecipeVault.Domain.Entities {
    [Table("RecipeNutrition")]
    public class RecipeNutrition : AuditableEntity {
        protected RecipeNutrition() {
        }

        public RecipeNutrition(int recipeId) {
            RecipeId = recipeId;
            CalculatedDate = DateTime.UtcNow;
            IsStale = false;
            IngredientsMatched = 0;
            IngredientsTotal = 0;
            CoveragePercent = 0;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeNutritionId { get; private set; }

        [ForeignKey(nameof(Recipe))]
        public int RecipeId { get; private set; }

        // Per-serving totals
        [Column(TypeName = "numeric(10,2)")]
        public decimal? CaloriesPerServing { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? ProteinPerServing { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? CarbsPerServing { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? FatPerServing { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? FiberPerServing { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? SugarPerServing { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? SodiumPerServing { get; private set; }

        // Coverage metrics
        public int IngredientsMatched { get; private set; }
        public int IngredientsTotal { get; private set; }

        [Column(TypeName = "numeric(5,2)")]
        public decimal CoveragePercent { get; private set; }

        public DateTime CalculatedDate { get; private set; }

        public bool IsStale { get; private set; }  // Recipe changed since calculation

        public virtual Recipe Recipe { get; private set; }

        public void SetPerServingValues(decimal? caloriesPerServing, decimal? proteinPerServing, 
            decimal? carbsPerServing, decimal? fatPerServing, decimal? fiberPerServing, 
            decimal? sugarPerServing, decimal? sodiumPerServing) {
            CaloriesPerServing = caloriesPerServing;
            ProteinPerServing = proteinPerServing;
            CarbsPerServing = carbsPerServing;
            FatPerServing = fatPerServing;
            FiberPerServing = fiberPerServing;
            SugarPerServing = sugarPerServing;
            SodiumPerServing = sodiumPerServing;
        }

        public void SetCoverageMetrics(int matched, int total) {
            IngredientsMatched = matched;
            IngredientsTotal = total;
            CoveragePercent = total > 0 ? Math.Round((decimal)matched / total * 100, 2) : 0;
        }

        public void MarkAsStale() {
            IsStale = true;
        }

        public void MarkAsFresh() {
            IsStale = false;
            CalculatedDate = DateTime.UtcNow;
        }
    }
}
