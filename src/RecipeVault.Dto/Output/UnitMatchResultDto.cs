namespace RecipeVault.Dto.Output {
    public class UnitMatchResultDto {
        public bool IsMatch { get; set; }
        public UnitDto Unit { get; set; }
        public decimal Confidence { get; set; }
        public string OriginalInput { get; set; }
    }
}
