using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public interface IUserPantryService {
        Task<List<UserPantryItem>> GetUserPantryAsync();
        Task<UserPantryItem> AddPantryItemAsync(CreatePantryItemDto dto);
        Task<UserPantryItem> UpdatePantryItemAsync(int id, UpdatePantryItemDto dto);
        Task DeletePantryItemAsync(int id);
        Task<List<string>> GetDefaultStaplesAsync();
        Task EnsureUserPantrySeededAsync();
    }
}
