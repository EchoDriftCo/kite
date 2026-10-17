using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class CircleDto {
        public Guid CircleResourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid OwnerSubjectId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CircleMemberDto> Members { get; set; }
        public List<CircleRecipeDto> SharedRecipes { get; set; }
        public int MemberCount { get; set; }
        public int RecipeCount { get; set; }
        public bool IsOwner { get; set; }
    }

    public class CircleMemberDto {
        public Guid SubjectId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime? JoinedDate { get; set; }
    }

    public class CircleRecipeDto {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public Guid SharedBySubjectId { get; set; }
        public string SharedByDisplayName { get; set; }
        public DateTime SharedDate { get; set; }
    }

    public class CircleInviteDto {
        public Guid InviteToken { get; set; }
        public string CircleName { get; set; }
        public string InviteeEmail { get; set; }
        public DateTime ExpiresDate { get; set; }
        public string Status { get; set; }
    }
}
