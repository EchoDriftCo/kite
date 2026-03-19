using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IApiTokenService {
        Task<ApiTokenCreatedDto> CreateTokenAsync(string name, int? expiresInDays);
        Task<List<ApiTokenDto>> ListTokensAsync();
        Task RevokeTokenAsync(Guid apiTokenResourceId);
    }
}
