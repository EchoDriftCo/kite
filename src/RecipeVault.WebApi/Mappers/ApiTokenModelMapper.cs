#pragma warning disable CS1591 // Missing XML comments

using System.Linq;
using RecipeVault.Dto.Output;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class ApiTokenModelMapper {
        public ApiTokenCreatedModel Map(ApiTokenCreatedDto dto) {
            if (dto == null) {
                return null;
            }

            return new ApiTokenCreatedModel {
                ApiTokenResourceId = dto.ApiTokenResourceId,
                Name = dto.Name,
                Token = dto.Token,
                TokenPrefix = dto.TokenPrefix,
                ExpiresDate = dto.ExpiresDate,
                CreatedDate = dto.CreatedDate
            };
        }

        public ApiTokenModel Map(ApiTokenDto dto) {
            if (dto == null) {
                return null;
            }

            return new ApiTokenModel {
                ApiTokenResourceId = dto.ApiTokenResourceId,
                Name = dto.Name,
                TokenPrefix = dto.TokenPrefix,
                CreatedDate = dto.CreatedDate,
                LastUsedDate = dto.LastUsedDate,
                ExpiresDate = dto.ExpiresDate,
                IsRevoked = dto.IsRevoked
            };
        }

        public ApiTokenListModel MapToList(System.Collections.Generic.List<ApiTokenDto> dtos) {
            return new ApiTokenListModel {
                Tokens = dtos?.Select(Map).ToList()
            };
        }
    }
}
