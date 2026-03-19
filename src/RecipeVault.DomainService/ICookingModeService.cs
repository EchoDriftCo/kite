using System;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface ICookingModeService {
        Task<CookingDataDto> GetCookingDataAsync(Recipe recipe);
    }
}
