using System;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Search {
    public class MealPlanSearchDto : SearchDto {
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
}
