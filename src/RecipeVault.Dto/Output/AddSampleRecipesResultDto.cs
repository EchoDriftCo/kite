using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class AddSampleRecipesResultDto {
        public int RecipesAdded { get; set; }
        public List<AddedRecipeDto> Recipes { get; set; }
    }
}
