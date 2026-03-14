using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class ApiTokenCreatedModel {
        public Guid ApiTokenResourceId { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string TokenPrefix { get; set; }
        public DateTime? ExpiresDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
