using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto;

namespace RecipeVault.Facade {
    public interface IPremiumWaitlistFacade {
        Task<PremiumWaitlistDto> JoinWaitlistAsync(string email, string source);
        Task<List<PremiumWaitlistDto>> GetAllAsync();
    }
}
