#pragma warning disable CS1591 // Missing XML comments

using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class CircleModel {
        public Guid CircleResourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid OwnerSubjectId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CircleMemberModel> Members { get; set; }
        public List<CircleRecipeModel> SharedRecipes { get; set; }
        public int MemberCount { get; set; }
        public int RecipeCount { get; set; }
        public bool IsOwner { get; set; }
    }

    public class CircleMemberModel {
        public Guid SubjectId { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime? JoinedDate { get; set; }
    }

    public class CircleRecipeModel {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public Guid SharedBySubjectId { get; set; }
        public DateTime SharedDate { get; set; }
    }

    public class CircleInviteModel {
        public Guid InviteToken { get; set; }
        public string CircleName { get; set; }
        public string InviteeEmail { get; set; }
        public DateTime ExpiresDate { get; set; }
        public string Status { get; set; }
    }
}
