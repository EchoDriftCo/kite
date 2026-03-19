using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Domain.Entities {
    [Table("MealPlanEntry")]
    public class MealPlanEntry {
        protected MealPlanEntry() {
        }

        public MealPlanEntry(DateTime date, MealSlot mealSlot, int recipeId, int? servings, bool isLeftover) {
            Date = date;
            MealSlot = mealSlot;
            RecipeId = recipeId;
            Servings = servings;
            IsLeftover = isLeftover;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealPlanEntryId { get; private set; }

        [ForeignKey(nameof(MealPlan))]
        public int MealPlanId { get; private set; }
        public virtual MealPlan MealPlan { get; private set; }

        public DateTime Date { get; private set; }

        public MealSlot MealSlot { get; private set; }

        [ForeignKey(nameof(Recipe))]
        public int RecipeId { get; private set; }
        public virtual Recipe Recipe { get; private set; }

        public int? Servings { get; private set; }

        public bool IsLeftover { get; private set; }
    }
}
