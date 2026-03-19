using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class MealPlanDto : AuditableEntityDto {
        public int MealPlanId { get; set; }
        public Guid MealPlanResourceId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<MealPlanEntryDto> Entries { get; set; }
    }

    public class MealPlanEntryDto {
        public int MealPlanEntryId { get; set; }
        public DateTime Date { get; set; }
        public int MealSlot { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string RecipeTitle { get; set; }
        public int RecipeYield { get; set; }
        public int? Servings { get; set; }
        public bool IsLeftover { get; set; }
    }
}
