using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Input {
    public class ExportRequestDto {
        public string Format { get; set; } // json, text, paprika
        public bool IncludeImages { get; set; } = true;
        public List<Guid> RecipeResourceIds { get; set; } = new List<Guid>();
    }
}
