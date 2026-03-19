using System.Linq;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade.Mappers {
    public class DietaryProfileMapper {
        public DietaryProfileDto MapToDto(DietaryProfile entity) {
            if (entity == null) {
                return null;
            }

            return new DietaryProfileDto {
                DietaryProfileResourceId = entity.DietaryProfileResourceId,
                SubjectId = entity.SubjectId,
                ProfileName = entity.ProfileName,
                IsDefault = entity.IsDefault,
                Restrictions = entity.Restrictions?.Select(r => new DietaryRestrictionDto {
                    RestrictionType = r.RestrictionType,
                    RestrictionCode = r.RestrictionCode,
                    Severity = r.Severity
                }).ToList(),
                AvoidedIngredients = entity.AvoidedIngredients?.Select(ai => new AvoidedIngredientDto {
                    AvoidedIngredientId = ai.AvoidedIngredientId,
                    IngredientName = ai.IngredientName,
                    Reason = ai.Reason
                }).ToList(),
                CreatedDate = entity.CreatedDate,
                LastModifiedDate = entity.LastModifiedDate
            };
        }
    }
}
