#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Responses {
    public class RecipeModel : AuditableEntityModel {
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
        public bool IsPublic { get; set; }
        public bool IsOwner { get; set; }
        public List<RecipeIngredientModel> Ingredients { get; set; }
        public List<RecipeInstructionModel> Instructions { get; set; }
        public List<RecipeTagModel> Tags { get; set; }
    }
}
