using System;

namespace RecipeVault.Dto.Output {
    public class AddedRecipeDto {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string Showcases { get; set; }
    }
}
