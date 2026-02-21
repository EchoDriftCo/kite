#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class AssignTagsModel {
        public List<AssignTagItemModel> Tags { get; set; }
    }

    public class AssignTagItemModel {
        public Guid? TagResourceId { get; set; }
        public string Name { get; set; }
        public int? Category { get; set; }
        
        [StringLength(100)]
        public string Alias { get; set; }
    }
}
