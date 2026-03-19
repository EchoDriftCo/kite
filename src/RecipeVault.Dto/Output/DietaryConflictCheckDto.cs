using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class DietaryConflictCheckDto {
        public bool CanEat { get; set; }
        public List<DietaryConflictDto> Conflicts { get; set; } = new();
    }

    public class DietaryConflictDto {
        public int IngredientIndex { get; set; }
        public string IngredientText { get; set; }
        public string RestrictionCode { get; set; }
        public string RestrictionType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }
}
