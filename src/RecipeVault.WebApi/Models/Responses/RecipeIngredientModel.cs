#pragma warning disable CS1591 // Missing XML comments

namespace RecipeVault.WebApi.Models.Responses {
    public class RecipeIngredientModel {
        public int RecipeIngredientId { get; set; }
        public int SortOrder { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string RawText { get; set; }
    }
}
