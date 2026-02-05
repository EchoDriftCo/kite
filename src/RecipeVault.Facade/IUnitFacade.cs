using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IUnitFacade {
        Task<IReadOnlyList<UnitDto>> GetAllAsync();
        Task<UnitDto> GetByIdAsync(Guid resourceId);
        Task<UnitMatchResultDto> MatchAsync(string input);
    }
}
