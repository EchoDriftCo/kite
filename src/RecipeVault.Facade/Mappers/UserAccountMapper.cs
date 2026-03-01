using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade.Mappers {
    public class UserAccountMapper {
        public UserAccountDto MapToDto(UserAccount entity) {
            if (entity == null) {
                return null;
            }

            return new UserAccountDto {
                UserAccountResourceId = entity.UserAccountResourceId,
                SubjectId = entity.SubjectId,
                AccountTier = entity.AccountTier.ToString(),
                CreatedDate = entity.CreatedDate,
                TierChangedDate = entity.TierChangedDate
            };
        }
    }
}
