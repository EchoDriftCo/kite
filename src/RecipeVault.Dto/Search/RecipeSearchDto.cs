using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Search {
    public class RecipeSearchDto : SearchDto {
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public bool? IsPublic { get; set; }
        public bool IncludePublic { get; set; }
        public List<Guid> TagResourceIds { get; set; }
        public int? TagCategory { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? HasRequiredEquipment { get; set; }
        public int? MinRating { get; set; }
        public Guid? CollectionResourceId { get; set; }
    }
}
