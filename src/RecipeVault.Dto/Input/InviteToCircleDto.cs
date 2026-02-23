using System;

namespace RecipeVault.Dto.Input {
    public class InviteToCircleDto {
        public string InviteeEmail { get; set; }
        public DateTime? ExpiresDate { get; set; }  // Optional, defaults to 7 days
    }
}
