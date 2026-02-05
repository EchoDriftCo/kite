using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IUnitRepository {
        Task<IReadOnlyList<Unit>> GetAllWithAliasesAsync();
        Task<Unit> GetByIdAsync(Guid resourceId);
        Task<Unit> GetByIdAsync(int unitId);
    }
}
