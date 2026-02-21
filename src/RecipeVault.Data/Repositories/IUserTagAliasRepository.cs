using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IUserTagAliasRepository {
        Task<UserTagAlias> AddAsync(UserTagAlias userTagAlias);
        Task<UserTagAlias> GetAsync(int userTagAliasId);
        Task<UserTagAlias> GetByUserAndTagAsync(Guid userId, int tagId);
        Task<List<UserTagAlias>> GetByUserIdAsync(Guid userId);
        Task<List<UserTagAlias>> GetByTagIdAsync(int tagId);
        Task RemoveAsync(UserTagAlias userTagAlias);
    }
}
