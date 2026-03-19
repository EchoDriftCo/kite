#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Requests {
    public class RecipeSearchModel : SearchModel {
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public bool? IsPublic { get; set; }
        public bool IncludePublic { get; set; }
        public List<Guid> TagResourceIds { get; set; }
        public int? TagCategory { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? HasRequiredEquipment { get; set; }
        public int? MinRating { get; set; }
        public Guid? CollectionResourceId { get; set; }
    }
}
