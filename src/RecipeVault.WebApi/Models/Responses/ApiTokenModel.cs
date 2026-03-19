using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class ApiTokenModel {
        public Guid ApiTokenResourceId { get; set; }
        public string Name { get; set; }
        public string TokenPrefix { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public DateTime? ExpiresDate { get; set; }
        public bool IsRevoked { get; set; }
    }
}
