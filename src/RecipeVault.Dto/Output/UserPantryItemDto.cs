using System;

namespace RecipeVault.Dto.Output {
    public class UserPantryItemDto {
        public int UserPantryItemId { get; set; }
        public string IngredientName { get; set; }
        public bool IsStaple { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
