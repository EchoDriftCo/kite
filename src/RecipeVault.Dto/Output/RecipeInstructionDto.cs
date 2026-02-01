namespace RecipeVault.Dto.Output {
    public class RecipeInstructionDto {
        public int RecipeInstructionId { get; set; }
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
    }
}
