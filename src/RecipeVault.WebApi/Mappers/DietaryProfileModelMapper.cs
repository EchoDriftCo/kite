using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class DietaryProfileModelMapper {
        public UpdateDietaryProfileDto MapToDto(UpdateDietaryProfileModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateDietaryProfileDto {
                ProfileName = model.ProfileName,
                IsDefault = model.IsDefault
            };
        }

        public AddDietaryRestrictionDto MapToDto(AddDietaryRestrictionModel model) {
            if (model == null) {
                return null;
            }

            return new AddDietaryRestrictionDto {
                RestrictionCode = model.RestrictionCode,
                RestrictionType = model.RestrictionType,
                Severity = model.Severity
            };
        }

        public AddAvoidedIngredientDto MapToDto(AddAvoidedIngredientModel model) {
            if (model == null) {
                return null;
            }

            return new AddAvoidedIngredientDto {
                IngredientName = model.IngredientName,
                Reason = model.Reason
            };
        }

        public DietaryProfileModel Map(DietaryProfileDto dto) {
            if (dto == null) {
                return null;
            }

            return new DietaryProfileModel {
                DietaryProfileResourceId = dto.DietaryProfileResourceId,
                SubjectId = dto.SubjectId,
                ProfileName = dto.ProfileName,
                IsDefault = dto.IsDefault,
                Restrictions = dto.Restrictions?.Select(r => new DietaryRestrictionModel {
                    RestrictionType = r.RestrictionType,
                    RestrictionCode = r.RestrictionCode,
                    Severity = r.Severity
                }).ToList(),
                AvoidedIngredients = dto.AvoidedIngredients?.Select(ai => new AvoidedIngredientModel {
                    AvoidedIngredientId = ai.AvoidedIngredientId,
                    IngredientName = ai.IngredientName,
                    Reason = ai.Reason
                }).ToList(),
                CreatedDate = dto.CreatedDate,
                LastModifiedDate = dto.LastModifiedDate
            };
        }

        public DietaryConflictCheckModel Map(DietaryConflictCheckDto dto) {
            if (dto == null) {
                return null;
            }

            return new DietaryConflictCheckModel {
                CanEat = dto.CanEat,
                Conflicts = dto.Conflicts?.Select(c => new DietaryConflictModel {
                    IngredientIndex = c.IngredientIndex,
                    IngredientText = c.IngredientText,
                    RestrictionCode = c.RestrictionCode,
                    RestrictionType = c.RestrictionType,
                    Severity = c.Severity,
                    Message = c.Message
                }).ToList()
            };
        }
    }
}
