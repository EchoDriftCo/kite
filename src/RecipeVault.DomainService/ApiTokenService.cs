using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;
using UUIDNext;

namespace RecipeVault.DomainService {
    public class ApiTokenService : IApiTokenService {
        private readonly IApiTokenRepository apiTokenRepository;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly ILogger<ApiTokenService> logger;

        public ApiTokenService(
            IApiTokenRepository apiTokenRepository,
            ISubjectPrincipal subjectPrincipal,
            ILogger<ApiTokenService> logger) {
            this.apiTokenRepository = apiTokenRepository;
            this.subjectPrincipal = subjectPrincipal;
            this.logger = logger;
        }

        public async Task<ApiTokenCreatedDto> CreateTokenAsync(string name, int? expiresInDays) {
            var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
            var (token, hash) = GenerateToken();

            var apiToken = new ApiToken(
                apiTokenResourceId: Uuid.NewDatabaseFriendly(Database.PostgreSql),
                subjectId: subjectId,
                name: name,
                tokenHash: hash,
                tokenPrefix: token[^8..],
                expiresDate: expiresInDays.HasValue
                    ? DateTime.UtcNow.AddDays(expiresInDays.Value)
                    : null
            );

            await apiTokenRepository.AddAsync(apiToken).ConfigureAwait(false);

            logger.LogInformation("API token created for subject {SubjectId} with prefix {TokenPrefix}", subjectId, apiToken.TokenPrefix);

            return new ApiTokenCreatedDto {
                ApiTokenResourceId = apiToken.ApiTokenResourceId,
                Name = name,
                Token = token,
                TokenPrefix = apiToken.TokenPrefix,
                ExpiresDate = apiToken.ExpiresDate,
                CreatedDate = apiToken.CreatedDate
            };
        }

        public async Task<List<ApiTokenDto>> ListTokensAsync() {
            var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
            var tokens = await apiTokenRepository
                .GetBySubjectIdAsync(subjectId).ConfigureAwait(false);

            return tokens.Select(t => new ApiTokenDto {
                ApiTokenResourceId = t.ApiTokenResourceId,
                Name = t.Name,
                TokenPrefix = t.TokenPrefix,
                CreatedDate = t.CreatedDate,
                LastUsedDate = t.LastUsedDate,
                ExpiresDate = t.ExpiresDate,
                IsRevoked = t.IsRevoked
            }).ToList();
        }

        public async Task RevokeTokenAsync(Guid apiTokenResourceId) {
            var subjectId = Guid.Parse(subjectPrincipal.SubjectId);
            var token = await apiTokenRepository
                .GetAsync(apiTokenResourceId).ConfigureAwait(false);

            if (token == null || token.SubjectId != subjectId) {
                throw new ApiTokenNotFoundException("Token not found");
            }

            token.Revoke();

            logger.LogInformation("API token revoked: {TokenPrefix}", token.TokenPrefix);
        }

        internal static (string token, string hash) GenerateToken() {
            var randomBytes = RandomNumberGenerator.GetBytes(32);
            var randomPart = Convert.ToBase64String(randomBytes)
                .Replace("+", "").Replace("/", "").Replace("=", "")[..40];
            var token = $"rv_{randomPart}";
            var hash = ComputeSha256(token);
            return (token, hash);
        }

        internal static string ComputeSha256(string input) {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
