using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade.Mappers {
    public class UserPantryMapper {
        public UserPantryItemDto MapToDto(UserPantryItem entity) {
            if (entity == null) {
                return null;
            }

            return new UserPantryItemDto {
                UserPantryItemId = entity.UserPantryItemId,
                IngredientName = entity.IngredientName,
                IsStaple = entity.IsStaple,
                ExpirationDate = entity.ExpirationDate
            };
        }
    }
}
