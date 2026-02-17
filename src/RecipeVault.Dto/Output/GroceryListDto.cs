using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class GroceryListDto {
        public List<GroceryItemDto> Items { get; set; }
    }

    public class GroceryItemDto {
        public string Item { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public List<string> Sources { get; set; } = new();
    }
}
