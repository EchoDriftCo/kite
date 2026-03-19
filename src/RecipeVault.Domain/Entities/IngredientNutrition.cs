using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;

namespace RecipeVault.Domain.Entities {
    [Table("IngredientNutrition")]
    public class IngredientNutrition : AuditableEntity {
        protected IngredientNutrition() {
        }

        public IngredientNutrition(int recipeIngredientId, int? fdcId, string matchedFoodName, decimal matchConfidence, decimal gramsUsed) {
            RecipeIngredientId = recipeIngredientId;
            FdcId = fdcId;
            MatchedFoodName = matchedFoodName;
            MatchConfidence = matchConfidence;
            GramsUsed = gramsUsed;
            CalculatedDate = DateTime.UtcNow;
            IsManualOverride = false;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IngredientNutritionId { get; private set; }

        [ForeignKey(nameof(RecipeIngredient))]
        public int RecipeIngredientId { get; private set; }

        // USDA reference
        public int? FdcId { get; private set; }  // USDA FoodData Central ID

        [StringLength(500)]
        public string MatchedFoodName { get; private set; }

        [Column(TypeName = "numeric(5,4)")]
        public decimal MatchConfidence { get; private set; }  // 0-1

        // Calculated values (per ingredient amount)
        [Column(TypeName = "numeric(10,2)")]
        public decimal? Calories { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? ProteinGrams { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? CarbsGrams { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? FatGrams { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? FiberGrams { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? SugarGrams { get; private set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? SodiumMg { get; private set; }

        // Conversion used
        [Column(TypeName = "numeric(10,3)")]
        public decimal GramsUsed { get; private set; }  // Amount in grams for calculation

        public DateTime CalculatedDate { get; private set; }

        public bool IsManualOverride { get; private set; }

        public virtual RecipeIngredient RecipeIngredient { get; private set; }

        public void SetNutritionValues(decimal? calories, decimal? proteinGrams, decimal? carbsGrams, 
            decimal? fatGrams, decimal? fiberGrams, decimal? sugarGrams, decimal? sodiumMg) {
            Calories = calories;
            ProteinGrams = proteinGrams;
            CarbsGrams = carbsGrams;
            FatGrams = fatGrams;
            FiberGrams = fiberGrams;
            SugarGrams = sugarGrams;
            SodiumMg = sodiumMg;
        }

        public void MarkAsManualOverride() {
            IsManualOverride = true;
        }
    }
}
