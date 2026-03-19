using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class CollectionDto {
        public Guid CollectionResourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        public int RecipeCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public List<CollectionRecipeDto> Recipes { get; set; }
    }

    public class CollectionRecipeDto {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SourceImageUrl { get; set; }
        public int SortOrder { get; set; }
        public DateTime AddedDate { get; set; }
        public int? TotalTimeMinutes { get; set; }
        public int? Rating { get; set; }
    }
}
