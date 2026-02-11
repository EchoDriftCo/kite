#pragma warning disable CS1591 // Missing XML comments

using System;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Requests {
    public class MealPlanSearchModel : SearchModel {
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
}
