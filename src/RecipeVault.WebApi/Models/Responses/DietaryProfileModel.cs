#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class DietaryProfileModel {
        public Guid DietaryProfileResourceId { get; set; }
        public Guid SubjectId { get; set; }
        public string ProfileName { get; set; }
        public bool IsDefault { get; set; }
        public List<DietaryRestrictionModel> Restrictions { get; set; } = new();
        public List<AvoidedIngredientModel> AvoidedIngredients { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }

    public class DietaryRestrictionModel {
        public string RestrictionType { get; set; }
        public string RestrictionCode { get; set; }
        public string Severity { get; set; }
    }

    public class AvoidedIngredientModel {
        public int AvoidedIngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Reason { get; set; }
    }

    public class DietaryConflictCheckModel {
        public bool CanEat { get; set; }
        public List<DietaryConflictModel> Conflicts { get; set; } = new();
    }

    public class DietaryConflictModel {
        public int IngredientIndex { get; set; }
        public string IngredientText { get; set; }
        public string RestrictionCode { get; set; }
        public string RestrictionType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }
}
