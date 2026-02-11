#pragma warning disable CS1591 // Missing XML comments

using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class GroceryListModel {
        public List<GroceryItemModel> Items { get; set; }
    }

    public class GroceryItemModel {
        public string Item { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
    }
}
