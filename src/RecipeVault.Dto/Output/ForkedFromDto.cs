using System;

namespace RecipeVault.Dto.Output {
    public class ForkedFromDto {
        public Guid RecipeResourceId { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public bool IsAvailable { get; set; }
    }
}
