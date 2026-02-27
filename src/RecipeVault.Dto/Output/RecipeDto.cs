using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class RecipeDto : AuditableEntityDto {
        public int RecipeId { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Yield { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public int? TotalTimeMinutes { get; set; }
        public string Source { get; set; }
        public string OriginalImageUrl { get; set; }
        public string SourceImageUrl { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; }
        public List<RecipeInstructionDto> Instructions { get; set; }
        public bool IsPublic { get; set; }
        public int? Rating { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsOwner { get; set; }
        public string ShareToken { get; set; }
        public List<RecipeTagDto> Tags { get; set; }
        public ForkedFromDto ForkedFrom { get; set; }
        public int ForkCount { get; set; }
        public MixedFromDto MixedFrom { get; set; }
    }
}
