using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public interface IDietaryProfileService {
        Task<DietaryProfile> CreateProfileAsync(UpdateDietaryProfileDto dto);
        Task<DietaryProfile> GetProfileAsync(Guid profileResourceId);
        Task<List<DietaryProfile>> GetProfilesBySubjectAsync();
        Task<DietaryProfile> UpdateProfileAsync(Guid resourceId, UpdateDietaryProfileDto dto);
        Task DeleteProfileAsync(Guid resourceId);
        Task<DietaryRestriction> AddRestrictionAsync(Guid profileResourceId, AddDietaryRestrictionDto dto);
        Task RemoveRestrictionAsync(Guid profileResourceId, string restrictionCode);
        Task<AvoidedIngredient> AddAvoidedIngredientAsync(Guid profileResourceId, AddAvoidedIngredientDto dto);
        Task RemoveAvoidedIngredientAsync(Guid profileResourceId, int avoidedIngredientId);
    }
}
