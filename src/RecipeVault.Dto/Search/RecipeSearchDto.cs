using System;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Search {
    public class RecipeSearchDto : SearchDto {
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public bool? IsPublic { get; set; }
        public bool IncludePublic { get; set; }
    }
}
