using System;
using System.Collections.Generic;
using Cortside.AspNetCore.Common.Dtos;

namespace RecipeVault.Dto.Output {
    public class CookingLogDto : AuditableEntityDto {
        public Guid CookingLogResourceId { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string RecipeTitle { get; set; }
        public DateTime CookedDate { get; set; }
        public decimal? ScaleFactor { get; set; }
        public int? ServingsMade { get; set; }
        public string Notes { get; set; }
        public int? Rating { get; set; }
        public List<CookingLogPhotoDto> Photos { get; set; }
    }

    public class CookingLogPhotoDto {
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int? SortOrder { get; set; }
    }
}
