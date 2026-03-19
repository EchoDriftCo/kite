#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class AddSampleRecipesResultModel {
        public int RecipesAdded { get; set; }
        public List<AddedRecipeModel> Recipes { get; set; }
    }

    public class AddedRecipeModel {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string Showcases { get; set; }
    }
}
