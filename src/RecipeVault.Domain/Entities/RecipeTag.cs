using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(RecipeId), nameof(TagId), IsUnique = true)]
    [Table("RecipeTag")]
    public class RecipeTag {
        protected RecipeTag() {
        }

        public RecipeTag(int recipeId, int tagId, Guid assignedBySubjectId, bool isAiAssigned, decimal? confidence) {
            RecipeId = recipeId;
            TagId = tagId;
            AssignedBySubjectId = assignedBySubjectId;
            IsAiAssigned = isAiAssigned;
            Confidence = confidence;
            IsOverridden = false;
            AssignedDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeTagId { get; private set; }

        [ForeignKey(nameof(Recipe))]
        public int RecipeId { get; private set; }
        public virtual Recipe Recipe { get; private set; }

        [ForeignKey(nameof(Tag))]
        public int TagId { get; private set; }
        public virtual Tag Tag { get; private set; }

        public Guid AssignedBySubjectId { get; private set; }

        public bool IsAiAssigned { get; private set; }

        [Column(TypeName = "numeric(3,2)")]
        public decimal? Confidence { get; private set; }

        public bool IsOverridden { get; private set; }

        public DateTime AssignedDate { get; private set; }

        public void MarkOverridden() {
            IsOverridden = true;
        }

        public void ClearOverride() {
            IsOverridden = false;
        }
    }
}
