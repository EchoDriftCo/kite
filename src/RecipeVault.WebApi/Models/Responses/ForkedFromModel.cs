#pragma warning disable CS1591 // Missing XML comments

using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class ForkedFromModel {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public bool IsAvailable { get; set; }
    }
}
