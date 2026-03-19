using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Dto.Input {
    public class ReorderCollectionsDto {
        [Required]
        public List<CollectionOrderDto> Collections { get; set; }
    }

    public class CollectionOrderDto {
        [Required]
        public Guid CollectionResourceId { get; set; }

        [Required]
        public int SortOrder { get; set; }
    }
}
