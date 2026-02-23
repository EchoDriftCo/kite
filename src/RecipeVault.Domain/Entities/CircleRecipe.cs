using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(CircleId), nameof(RecipeId), IsUnique = true)]
    [Index(nameof(RecipeId))]
    [Table("CircleRecipe")]
    public class CircleRecipe {
        protected CircleRecipe() {
        }

        public CircleRecipe(int circleId, int recipeId, int sharedBySubjectId) {
            CircleId = circleId;
            RecipeId = recipeId;
            SharedBySubjectId = sharedBySubjectId;
            SharedDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleRecipeId { get; private set; }

        public int CircleId { get; private set; }
        public virtual Circle Circle { get; private set; }

        public int RecipeId { get; private set; }
        public virtual Recipe Recipe { get; private set; }

        public int SharedBySubjectId { get; private set; }

        public DateTime SharedDate { get; private set; }
    }
}
