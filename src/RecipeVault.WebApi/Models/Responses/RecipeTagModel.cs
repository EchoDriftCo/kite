#pragma warning disable CS1591 // Missing XML comments

using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class RecipeTagModel {
        public Guid TagResourceId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public bool IsAiAssigned { get; set; }
        public decimal? Confidence { get; set; }
        public bool IsOverridden { get; set; }
    }
}
