using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Input {
    public class UpdateMealPlanDto {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<UpdateMealPlanEntryDto> Entries { get; set; }
    }

    public class UpdateMealPlanEntryDto {
        public DateTime Date { get; set; }
        public int MealSlot { get; set; }
        public Guid RecipeResourceId { get; set; }
        public int? Servings { get; set; }
        public bool IsLeftover { get; set; }
    }
}
