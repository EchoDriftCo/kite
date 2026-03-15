using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IUserPantryRepository {
        Task<List<UserPantryItem>> GetBySubjectIdAsync(Guid subjectId);
        Task<UserPantryItem> GetByIdAsync(int id);
        Task<List<string>> GetStaplesAsync(Guid subjectId);
        Task<int> CountAsync(Guid subjectId);
        Task<UserPantryItem> AddAsync(UserPantryItem item);
        Task AddRangeAsync(List<UserPantryItem> items);
        void Remove(UserPantryItem item);
    }
}
