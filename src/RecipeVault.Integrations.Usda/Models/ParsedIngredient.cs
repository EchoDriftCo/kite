namespace RecipeVault.Integrations.Usda.Models {
    public class ParsedIngredient {
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
        public string OriginalText { get; set; }
    }
}
