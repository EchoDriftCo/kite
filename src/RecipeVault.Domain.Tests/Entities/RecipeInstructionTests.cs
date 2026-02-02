using Shouldly;
using Xunit;
using RecipeVault.TestUtilities.Builders;

namespace RecipeVault.Domain.Tests.Entities {
    public class RecipeInstructionTests {
        [Fact]
        public void RecipeInstruction_CreatedWithValidValues_HasValidProperties() {
            // Arrange & Act
            var instruction = new RecipeInstructionBuilder()
                .WithStepNumber(1)
                .WithInstruction("Preheat oven to 350°F")
                .WithRawText("Preheat oven to 350°F")
                .Build();

            // Assert
            instruction.StepNumber.ShouldBe(1);
            instruction.Instruction.ShouldBe("Preheat oven to 350°F");
            instruction.RawText.ShouldBe("Preheat oven to 350°F");
        }

        [Fact]
        public void RecipeInstruction_MultipleSteps_HaveDifferentStepNumbers() {
            // Arrange & Act
            var step1 = new RecipeInstructionBuilder().WithStepNumber(1).Build();
            var step2 = new RecipeInstructionBuilder().WithStepNumber(2).Build();
            var step3 = new RecipeInstructionBuilder().WithStepNumber(3).Build();

            // Assert
            step1.StepNumber.ShouldBe(1);
            step2.StepNumber.ShouldBe(2);
            step3.StepNumber.ShouldBe(3);
        }
    }
}
