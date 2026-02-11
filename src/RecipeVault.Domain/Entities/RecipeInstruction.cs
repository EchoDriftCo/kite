using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;

namespace RecipeVault.Domain.Entities {
    [Table("RecipeInstruction")]
    public class RecipeInstruction : AuditableEntity {
        protected RecipeInstruction() {
        }

        public RecipeInstruction(int stepNumber, string instruction, string rawText) {
            StepNumber = stepNumber;
            Instruction = instruction;
            RawText = string.IsNullOrWhiteSpace(rawText) ? instruction : rawText;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeInstructionId { get; private set; }

        [ForeignKey(nameof(RecipeId))]
        public int RecipeId { get; private set; }

        public int StepNumber { get; private set; }

        [Required]
        public string Instruction { get; private set; }

        [Required]
        public string RawText { get; private set; }
    }
}
