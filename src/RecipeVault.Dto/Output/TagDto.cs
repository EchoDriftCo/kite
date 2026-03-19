using System;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class TagDto : AuditableEntityDto {
        public int TagId { get; set; }
        public Guid TagResourceId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public bool IsGlobal { get; set; }
        public int? SourceType { get; set; }
        public string SourceTypeName { get; set; }
        public bool IsSystemTag { get; set; }
        
        // User alias fields (null if no alias for current user)
        public string Alias { get; set; }
        public bool ShowAliasPublicly { get; set; }
        public string NormalizedEntityId { get; set; }
        public int? NormalizedEntityType { get; set; }
        public string NormalizedEntityTypeName { get; set; }
    }
}
