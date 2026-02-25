using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class DietaryProfileDto {
        public Guid DietaryProfileResourceId { get; set; }
        public Guid SubjectId { get; set; }
        public string ProfileName { get; set; }
        public bool IsDefault { get; set; }
        public List<DietaryRestrictionDto> Restrictions { get; set; } = new();
        public List<AvoidedIngredientDto> AvoidedIngredients { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
