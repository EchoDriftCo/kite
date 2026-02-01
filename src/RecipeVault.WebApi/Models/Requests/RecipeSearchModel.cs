#pragma warning disable CS1591 // Missing XML comments

using System;
using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Requests {
    public class RecipeSearchModel : SearchModel {
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
    }
}
