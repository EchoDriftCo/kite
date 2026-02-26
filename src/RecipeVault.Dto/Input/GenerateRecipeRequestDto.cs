using System.Collections.Generic;

namespace RecipeVault.Dto.Input {
    /// <summary>
    /// Request DTO for generating a new recipe using AI
    /// </summary>
    public class GenerateRecipeRequestDto {
        /// <summary>
        /// Natural language description of desired recipe (required)
        /// e.g., "A creamy pasta with mushrooms and bacon for date night"
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Maximum total time in minutes (optional)
        /// </summary>
        public int? MaxTime { get; set; }

        /// <summary>
        /// Dietary constraints to apply (optional)
        /// e.g., ["Vegetarian", "Gluten-Free"]
        /// </summary>
        public List<string> Dietary { get; set; } = new();

        /// <summary>
        /// Skill level constraint (optional)
        /// e.g., "beginner", "intermediate", "advanced"
        /// </summary>
        public string SkillLevel { get; set; }

        /// <summary>
        /// Number of recipe variations to generate (1-3, default: 1)
        /// </summary>
        public int Variations { get; set; } = 1;
    }
}
