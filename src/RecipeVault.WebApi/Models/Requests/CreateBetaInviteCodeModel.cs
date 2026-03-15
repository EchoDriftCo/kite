#pragma warning disable CS1591 // Missing XML comments

using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class CreateBetaInviteCodeModel {
        [Range(1, 100)]
        public int Count { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int MaxUses { get; set; } = 1;

        public DateTime? ExpiresDate { get; set; }
    }
}
