using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IUserAccountFacade {
        Task<UserAccountDto> GetCurrentAccountAsync();
        Task<UserAccountDto> SetTierAsync(string tier);
    }
}
