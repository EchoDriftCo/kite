using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IDietaryProfileFacade {
        Task<DietaryProfileDto> CreateProfileAsync(UpdateDietaryProfileDto dto);
        Task<DietaryProfileDto> GetProfileAsync(Guid resourceId);
        Task<List<DietaryProfileDto>> GetProfilesBySubjectAsync();
        Task<DietaryProfileDto> UpdateProfileAsync(Guid resourceId, UpdateDietaryProfileDto dto);
        Task DeleteProfileAsync(Guid resourceId);
        Task<DietaryProfileDto> AddRestrictionAsync(Guid profileResourceId, AddDietaryRestrictionDto dto);
        Task RemoveRestrictionAsync(Guid profileResourceId, string restrictionCode);
        Task<DietaryProfileDto> AddAvoidedIngredientAsync(Guid profileResourceId, AddAvoidedIngredientDto dto);
        Task RemoveAvoidedIngredientAsync(Guid profileResourceId, int avoidedIngredientId);
        Task<DietaryConflictCheckDto> CheckRecipeAsync(Guid recipeResourceId, Guid? profileResourceId = null);
    }
}
