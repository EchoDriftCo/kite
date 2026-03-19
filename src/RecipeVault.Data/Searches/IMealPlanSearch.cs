using System;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public interface IMealPlanSearch : ISearch, ISearchBuilder<MealPlan> {
        Guid? CreatedSubjectId { get; set; }
        DateTime? StartDateFrom { get; set; }
        DateTime? StartDateTo { get; set; }
    }
}
