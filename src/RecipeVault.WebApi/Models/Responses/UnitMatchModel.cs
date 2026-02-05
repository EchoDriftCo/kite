namespace RecipeVault.WebApi.Models.Responses {
    public class UnitMatchModel {
        public bool IsMatch { get; set; }
        public UnitModel Unit { get; set; }
        public decimal Confidence { get; set; }
        public string OriginalInput { get; set; }
    }
}
