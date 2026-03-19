using System;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Search {
    public class TagSearchDto : SearchDto {
        public string Name { get; set; }
        public int? Category { get; set; }
        public bool? IsGlobal { get; set; }
        public Guid? CreatedSubjectId { get; set; }
    }
}
