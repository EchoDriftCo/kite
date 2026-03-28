using System.Threading.Tasks;
using RecipeVault.Dto;

namespace RecipeVault.Facade {
    public interface IPremiumWaitlistFacade {
        Task<PremiumWaitlistDto> JoinWaitlistAsync(string email, string source);
    }
}
