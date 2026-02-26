using System;

namespace RecipeVault.Dto.Output {
    public class UsedInRecipeDto {
        public Guid RecipeLinkResourceId { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string OriginalImageUrl { get; set; }
        public string OwnerName { get; set; }
    }
}
