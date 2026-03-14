using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IApiTokenRepository {
        Task<ApiToken> AddAsync(ApiToken apiToken);
        Task<ApiToken> GetAsync(Guid apiTokenResourceId);
        Task<ApiToken> GetByHashAsync(string tokenHash);
        Task<List<ApiToken>> GetBySubjectIdAsync(Guid subjectId);
    }
}
