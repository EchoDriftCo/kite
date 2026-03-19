#pragma warning disable CS1591 // Missing XML comments

using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Requests {
    public class TagSearchModel : SearchModel {
        public string Name { get; set; }
        public int? Category { get; set; }
        public bool? IsGlobal { get; set; }
    }
}
