#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Responses {
    public class MealPlanModel : AuditableEntityModel {
        public Guid MealPlanResourceId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<MealPlanEntryModel> Entries { get; set; }
    }

    public class MealPlanEntryModel {
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
