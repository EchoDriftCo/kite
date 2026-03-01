#pragma warning disable CS1591 // Missing XML comments

using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class RedeemInviteCodeModel {
        [Required]
        [StringLength(20)]
        public string Code { get; set; }
    }
}
