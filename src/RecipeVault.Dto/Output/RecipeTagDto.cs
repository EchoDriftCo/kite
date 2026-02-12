using System;

namespace RecipeVault.Dto.Output {
    public class RecipeTagDto {
        public Guid TagResourceId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public bool IsAiAssigned { get; set; }
        public decimal? Confidence { get; set; }
        public bool IsOverridden { get; set; }
    }
}
