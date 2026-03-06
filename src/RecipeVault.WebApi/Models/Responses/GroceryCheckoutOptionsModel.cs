using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class GroceryCheckoutOptionsModel {
        public int ItemCount { get; set; }
        public List<string> NormalizedItems { get; set; }
        public List<GroceryCheckoutUrlModel> Options { get; set; }
    }
}
