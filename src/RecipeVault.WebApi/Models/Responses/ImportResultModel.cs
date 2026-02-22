#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class ImportResultModel {
        public int TotalRecipes { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportedRecipeModel> ImportedRecipes { get; set; }
        public List<ImportErrorModel> Errors { get; set; }
    }

    public class ImportedRecipeModel {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
    }

    public class ImportErrorModel {
        public string RecipeName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
