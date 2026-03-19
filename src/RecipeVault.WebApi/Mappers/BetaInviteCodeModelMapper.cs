#pragma warning disable CS1591 // Missing XML comments

using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class BetaInviteCodeModelMapper {
        private readonly UserAccountModelMapper userAccountMapper;

        public BetaInviteCodeModelMapper(UserAccountModelMapper userAccountMapper) {
            this.userAccountMapper = userAccountMapper;
        }

        public BetaInviteCodeModel Map(BetaInviteCodeDto dto) {
            if (dto == null) {
                return null;
            }

            return new BetaInviteCodeModel {
                BetaInviteCodeResourceId = dto.BetaInviteCodeResourceId,
                Code = dto.Code,
                MaxUses = dto.MaxUses,
                UseCount = dto.UseCount,
                IsActive = dto.IsActive,
                CreatedDate = dto.CreatedDate,
                ExpiresDate = dto.ExpiresDate
            };
        }

        public RedeemCodeResultModel Map(RedeemCodeResultDto dto) {
            if (dto == null) {
                return null;
            }

            return new RedeemCodeResultModel {
                Success = dto.Success,
                ErrorMessage = dto.ErrorMessage,
                UpdatedAccount = userAccountMapper.Map(dto.UpdatedAccount)
            };
        }

        public ValidateInviteCodeResultModel Map(ValidateInviteCodeResultDto dto) {
            if (dto == null) {
                return null;
            }

            return new ValidateInviteCodeResultModel {
                IsValid = dto.IsValid,
                Message = dto.Message
            };
        }

        public BetaInviteCodeSearchDto MapToDto(BetaInviteCodeSearchModel model) {
            if (model == null) {
                return null;
            }

            return new BetaInviteCodeSearchDto {
                IsActive = model.IsActive,
                PageNumber = model.PageNumber > 0 ? model.PageNumber : 1,
                PageSize = model.PageSize > 0 ? model.PageSize : 20,
                Sort = model.Sort
            };
        }

        public CreateBetaInviteCodeDto MapToDto(CreateBetaInviteCodeModel model) {
            if (model == null) {
                return null;
            }

            return new CreateBetaInviteCodeDto {
                Count = model.Count,
                MaxUses = model.MaxUses,
                ExpiresDate = model.ExpiresDate
            };
        }

        public RedeemInviteCodeDto MapToDto(RedeemInviteCodeModel model) {
            if (model == null) {
                return null;
            }

            return new RedeemInviteCodeDto {
                Code = model.Code
            };
        }
    }
}
