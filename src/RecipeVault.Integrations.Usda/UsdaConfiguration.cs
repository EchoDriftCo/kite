namespace RecipeVault.Integrations.Usda {
    public class UsdaConfiguration {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.nal.usda.gov/fdc/v1";
    }
}
