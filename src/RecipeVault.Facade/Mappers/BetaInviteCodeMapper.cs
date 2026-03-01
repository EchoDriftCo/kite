using System.Linq;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade.Mappers {
    public class BetaInviteCodeMapper {
        public BetaInviteCodeDto MapToDto(BetaInviteCode entity) {
            if (entity == null) {
                return null;
            }

            return new BetaInviteCodeDto {
                BetaInviteCodeResourceId = entity.BetaInviteCodeResourceId,
                Code = entity.Code,
                MaxUses = entity.MaxUses,
                UseCount = entity.UseCount,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                ExpiresDate = entity.ExpiresDate,
                Redemptions = entity.Redemptions?.Select(r => new BetaInviteCodeRedemptionDto {
                    SubjectId = r.SubjectId,
                    RedeemedDate = r.RedeemedDate
                }).ToList()
            };
        }

        public BetaInviteCodeSearch Map(BetaInviteCodeSearchDto dto) {
            if (dto == null) {
                return null;
            }

            return new BetaInviteCodeSearch {
                IsActive = dto.IsActive,
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                Sort = dto.Sort
            };
        }
    }
}
