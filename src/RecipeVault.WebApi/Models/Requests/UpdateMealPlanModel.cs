#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class UpdateMealPlanModel {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public List<UpdateMealPlanEntryModel> Entries { get; set; }
    }

    public class UpdateMealPlanEntryModel {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int MealSlot { get; set; }
        [Required]
        public Guid RecipeResourceId { get; set; }
        public int? Servings { get; set; }
        public bool IsLeftover { get; set; }
    }
}
