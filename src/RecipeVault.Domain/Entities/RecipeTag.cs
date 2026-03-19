using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(RecipeId), nameof(TagId), IsUnique = true)]
    [Table("RecipeTag")]
    public class RecipeTag {
        protected RecipeTag() {
        }

        public RecipeTag(int recipeId, int tagId, Guid assignedBySubjectId, bool isAiAssigned, decimal? confidence, string detail = null) {
            RecipeId = recipeId;
            TagId = tagId;
            AssignedBySubjectId = assignedBySubjectId;
            IsAiAssigned = isAiAssigned;
            Confidence = confidence;
            Detail = detail;
            IsOverridden = false;
            AssignedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a RecipeTag using a Tag entity reference.
        /// Use this when the Tag may not yet be persisted (TagId not assigned).
        /// EF Core will resolve the relationship on SaveChanges.
        /// </summary>
        public RecipeTag(int recipeId, Tag tag, Guid assignedBySubjectId, bool isAiAssigned, decimal? confidence, string detail = null) {
            RecipeId = recipeId;
            Tag = tag;
            AssignedBySubjectId = assignedBySubjectId;
            IsAiAssigned = isAiAssigned;
            Confidence = confidence;
            Detail = detail;
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

        [StringLength(100)]
        public string Detail { get; private set; }

        [StringLength(100)]
        public string NormalizedEntityId { get; private set; }

        public SourceType? NormalizedEntityType { get; private set; }

        public void MarkOverridden() {
            IsOverridden = true;
        }

        public void ClearOverride() {
            IsOverridden = false;
        }

        public void UpdateDetail(string detail) {
            Detail = detail;
        }

        public void SetNormalizedEntity(string entityId, SourceType entityType) {
            NormalizedEntityId = entityId;
            NormalizedEntityType = entityType;
        }

        public void ClearNormalizedEntity() {
            NormalizedEntityId = null;
            NormalizedEntityType = null;
        }
    }
}
