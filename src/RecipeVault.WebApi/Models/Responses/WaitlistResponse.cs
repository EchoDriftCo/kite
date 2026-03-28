using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class WaitlistResponse {
        public string Message { get; set; }
        public Guid? ResourceId { get; set; }
    }
}
