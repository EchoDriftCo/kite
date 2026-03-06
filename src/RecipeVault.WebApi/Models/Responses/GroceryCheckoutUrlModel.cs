using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class GroceryCheckoutUrlModel {
        public string Service { get; set; }
        public string Url { get; set; }
        public int ItemCount { get; set; }
        public List<string> NormalizedItems { get; set; }
    }
}
