using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Enums;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(UserId), nameof(TagId), IsUnique = true)]
    [Index(nameof(UserId))]
    [Index(nameof(TagId))]
    [Table("UserTagAlias")]
    public class UserTagAlias : AuditableEntity {
        protected UserTagAlias() {
        }

        public UserTagAlias(Guid userId, int tagId, string alias, bool showAliasPublicly = false) {
            UserId = userId;
            TagId = tagId;
            Alias = alias;
            ShowAliasPublicly = showAliasPublicly;
        }

        public UserTagAlias(Guid userId, Tag tag, string alias, bool showAliasPublicly = false) {
            UserId = userId;
            Tag = tag;
            Alias = alias;
            ShowAliasPublicly = showAliasPublicly;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserTagAliasId { get; private set; }

        public Guid UserId { get; private set; }

        [ForeignKey(nameof(Tag))]
        public int TagId { get; private set; }
        public virtual Tag Tag { get; private set; }

        [Required]
        [StringLength(100)]
        public string Alias { get; private set; }

        [StringLength(100)]
        public string NormalizedEntityId { get; private set; }

        public SourceType? NormalizedEntityType { get; private set; }

        public bool ShowAliasPublicly { get; private set; }

        public void UpdateAlias(string alias, bool showAliasPublicly) {
            Alias = alias;
            ShowAliasPublicly = showAliasPublicly;
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
