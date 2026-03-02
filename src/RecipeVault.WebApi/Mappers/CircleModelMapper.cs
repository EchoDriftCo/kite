using System.Linq;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.WebApi.Models.Requests;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Mappers {
    public class CircleModelMapper {
        public CircleModel Map(CircleDto dto) {
            if (dto == null) {
                return null;
            }

            return new CircleModel {
                CircleResourceId = dto.CircleResourceId,
                Name = dto.Name,
                Description = dto.Description,
                OwnerSubjectId = dto.OwnerSubjectId,
                CreatedDate = dto.CreatedDate,
                Members = dto.Members?.Select(m => new CircleMemberModel {
                    SubjectId = m.SubjectId,
                    Role = m.Role,
                    Status = m.Status,
                    JoinedDate = m.JoinedDate
                }).ToList(),
                SharedRecipes = dto.SharedRecipes?.Select(sr => new CircleRecipeModel {
                    RecipeResourceId = sr.RecipeResourceId,
                    Title = sr.Title,
                    SharedBySubjectId = sr.SharedBySubjectId,
                    SharedDate = sr.SharedDate
                }).ToList(),
                MemberCount = dto.MemberCount,
                RecipeCount = dto.RecipeCount,
                IsOwner = dto.IsOwner
            };
        }

        public CircleInviteModel Map(CircleInviteDto dto) {
            if (dto == null) {
                return null;
            }

            return new CircleInviteModel {
                InviteToken = dto.InviteToken,
                CircleName = dto.CircleName,
                InviteeEmail = dto.InviteeEmail,
                ExpiresDate = dto.ExpiresDate,
                Status = dto.Status
            };
        }

        public UpdateCircleDto MapToDto(UpdateCircleModel model) {
            if (model == null) {
                return null;
            }

            return new UpdateCircleDto {
                Name = model.Name,
                Description = model.Description
            };
        }

        public InviteToCircleDto MapToDto(InviteToCircleModel model) {
            if (model == null) {
                return null;
            }

            return new InviteToCircleDto {
                InviteeEmail = model.InviteeEmail,
                ExpiresDate = model.ExpiresDate
            };
        }

        public ShareRecipeToCircleDto MapToDto(ShareRecipeToCircleModel model) {
            if (model == null) {
                return null;
            }

            return new ShareRecipeToCircleDto {
                RecipeResourceId = model.RecipeResourceId
            };
        }

        public CircleSearchDto MapToDto(CircleSearchModel model) {
            if (model == null) {
                return null;
            }

            return new CircleSearchDto {
                OwnedOnly = model.OwnedOnly,
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                Sort = model.Sort
            };
        }
    }
}
