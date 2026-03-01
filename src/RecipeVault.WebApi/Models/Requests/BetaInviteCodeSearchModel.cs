#pragma warning disable CS1591 // Missing XML comments

using Cortside.AspNetCore.Common.Models;

namespace RecipeVault.WebApi.Models.Requests {
    public class BetaInviteCodeSearchModel : SearchModel {
        public bool? IsActive { get; set; }
    }
}
