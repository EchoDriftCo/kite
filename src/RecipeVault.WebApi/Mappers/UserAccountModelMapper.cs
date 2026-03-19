using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class UserAccountModelMapper {
        public UserAccountModel Map(UserAccountDto dto) {
            if (dto == null) {
                return null;
            }

            return new UserAccountModel {
                UserAccountResourceId = dto.UserAccountResourceId,
                SubjectId = dto.SubjectId,
                AccountTier = dto.AccountTier,
                CreatedDate = dto.CreatedDate,
                TierChangedDate = dto.TierChangedDate
            };
        }

        public SetTierDto MapToDto(SetTierModel model) {
            if (model == null) {
                return null;
            }

            return new SetTierDto {
                Tier = model.Tier
            };
        }
    }
}
