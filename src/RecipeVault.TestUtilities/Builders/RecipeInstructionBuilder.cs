using Bogus;
using RecipeVault.Domain.Entities;

namespace RecipeVault.TestUtilities.Builders {
    public class RecipeInstructionBuilder {
        private int _stepNumber = 1;
        private string _instruction = "Mix ingredients together";
        private string _rawText = "Mix ingredients together";

        public RecipeInstructionBuilder WithStepNumber(int stepNumber) {
            _stepNumber = stepNumber;
            return this;
        }

        public RecipeInstructionBuilder WithInstruction(string instruction) {
            _instruction = instruction;
            return this;
        }

        public RecipeInstructionBuilder WithRawText(string rawText) {
            _rawText = rawText;
            return this;
        }

        public RecipeInstructionBuilder WithRandomValues() {
            var faker = new Faker();
            _stepNumber = faker.Random.Int(1, 20);
            _instruction = faker.Lorem.Sentence();
            _rawText = _instruction;
            return this;
        }

        public RecipeInstruction Build() {
            return new RecipeInstruction(_stepNumber, _instruction, _rawText);
        }
    }
}
