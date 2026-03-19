#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Responses {
    public class CookingLogModel : AuditableEntityModel {
        public Guid CookingLogResourceId { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string RecipeTitle { get; set; }
        public DateTime CookedDate { get; set; }
        public decimal? ScaleFactor { get; set; }
        public int? ServingsMade { get; set; }
        public string Notes { get; set; }
        public int? Rating { get; set; }
        public List<CookingLogPhotoModel> Photos { get; set; }
    }

    public class CookingLogPhotoModel {
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int? SortOrder { get; set; }
    }
}
