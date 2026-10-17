using System;

namespace RecipeVault.Dto {
    public class PremiumWaitlistDto {
        public Guid PremiumWaitlistResourceId { get; set; }
        public string Email { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
