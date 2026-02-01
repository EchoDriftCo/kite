#pragma warning disable CS1591 // Missing XML comments

namespace RecipeVault.WebApi.Models.Responses {
    public class RecipeInstructionModel {
        public int RecipeInstructionId { get; set; }
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public string RawText { get; set; }
    }
}
