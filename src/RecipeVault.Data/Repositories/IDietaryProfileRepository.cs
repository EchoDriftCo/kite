using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IDietaryProfileRepository {
        Task<DietaryProfile> AddAsync(DietaryProfile profile);
        Task<DietaryProfile> GetAsync(Guid profileResourceId);
        Task<DietaryProfile> GetByIdAsync(int profileId);
        Task<List<DietaryProfile>> GetProfilesBySubjectAsync(Guid subjectId);
        Task<DietaryProfile> GetDefaultProfileBySubjectAsync(Guid subjectId);
        Task RemoveAsync(DietaryProfile profile);
        Task<DietaryRestriction> GetRestrictionAsync(int profileId, string restrictionCode);
        Task<AvoidedIngredient> GetAvoidedIngredientAsync(int profileId, int avoidedIngredientId);
    }
}
