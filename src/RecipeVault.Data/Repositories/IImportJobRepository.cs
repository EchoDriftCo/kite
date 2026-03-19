using System;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IImportJobRepository {
        void Add(ImportJob job);
        Task<ImportJob> GetByResourceIdAsync(Guid resourceId);
    }
}
