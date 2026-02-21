#pragma warning disable CS1591 // Missing XML comments

using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class RecipeTagModel {
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
        public string Detail { get; set; }
        public string NormalizedEntityId { get; set; }
        public int? NormalizedEntityType { get; set; }
    }
}
