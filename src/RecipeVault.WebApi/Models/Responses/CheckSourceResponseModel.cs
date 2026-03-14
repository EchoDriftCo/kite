using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class CheckSourceResponseModel {
        public bool Exists { get; set; }
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
