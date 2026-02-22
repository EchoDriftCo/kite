using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class ImportResultDto {
        public int TotalRecipes { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportedRecipeDto> ImportedRecipes { get; set; } = new();
        public List<ImportErrorDto> Errors { get; set; } = new();
    }

    public class ImportedRecipeDto {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
    }

    public class ImportErrorDto {
        public string RecipeName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
