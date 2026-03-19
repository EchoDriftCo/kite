using System;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class RecipeLinkDto : AuditableEntityDto {
        public Guid RecipeLinkResourceId { get; set; }
        public int ParentRecipeId { get; set; }
        public int LinkedRecipeId { get; set; }
        public int? IngredientIndex { get; set; }
        public string DisplayText { get; set; }
        public bool IncludeInTotalTime { get; set; }
        public decimal? PortionUsed { get; set; }
    }
}
