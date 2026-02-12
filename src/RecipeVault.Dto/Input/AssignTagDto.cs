using System;

namespace RecipeVault.Dto.Input {
    public class AssignTagDto {
        public Guid? TagResourceId { get; set; }
        public string Name { get; set; }
        public int? Category { get; set; }
    }
}
