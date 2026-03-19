using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IUserPantryFacade {
        Task<List<UserPantryItemDto>> GetUserPantryAsync();
        Task<UserPantryItemDto> AddPantryItemAsync(CreatePantryItemDto dto);
        Task<UserPantryItemDto> UpdatePantryItemAsync(int id, UpdatePantryItemDto dto);
        Task DeletePantryItemAsync(int id);
        Task<List<string>> GetDefaultStaplesAsync();
    }
}
