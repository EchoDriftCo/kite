using System;

namespace RecipeVault.Dto.Output {
    public class CheckSourceDto {
        public bool Exists { get; set; }
        public Guid? RecipeResourceId { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
