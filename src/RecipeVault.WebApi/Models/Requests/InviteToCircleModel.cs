#pragma warning disable CS1591 // Missing XML comments

using System;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.WebApi.Models.Requests {
    public class InviteToCircleModel {
        [EmailAddress]
        public string InviteeEmail { get; set; }
        
        public DateTime? ExpiresDate { get; set; }
    }
}
