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
    }
}
