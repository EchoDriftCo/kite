namespace RecipeVault.Dto.Input {
    public class UpdateRecipeLinkDto {
        public int? IngredientIndex { get; set; }
        public string DisplayText { get; set; }
        public bool IncludeInTotalTime { get; set; }
        public decimal? PortionUsed { get; set; }
    }
}
