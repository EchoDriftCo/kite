using System;

namespace RecipeVault.Dto.Output {
    public class RecipeTagDto {
        public Guid TagResourceId { get; set; }
        public string GlobalName { get; set; }
        public string DisplayName { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public int? SourceType { get; set; }
        public string SourceTypeName { get; set; }
        public bool IsAiAssigned { get; set; }
        public decimal? Confidence { get; set; }
        public bool IsOverridden { get; set; }
        public bool IsOwnerAlias { get; set; }
    }
}
