using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.DomainService {
    public interface IPremiumWaitlistService {
        Task<PremiumWaitlist> JoinWaitlistAsync(string email, string source);
    }
}
