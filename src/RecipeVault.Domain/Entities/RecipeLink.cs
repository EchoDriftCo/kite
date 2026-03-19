using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Microsoft.EntityFrameworkCore;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(RecipeLinkResourceId), IsUnique = true)]
    [Index(nameof(ParentRecipeId))]
    [Index(nameof(LinkedRecipeId))]
    [Table("RecipeLink")]
    public class RecipeLink : AuditableEntity {
        protected RecipeLink() {
        }

        public RecipeLink(int parentRecipeId, int linkedRecipeId, int? ingredientIndex, string displayText, bool includeInTotalTime, decimal? portionUsed) {
            RecipeLinkResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            ParentRecipeId = parentRecipeId;
            LinkedRecipeId = linkedRecipeId;
            IngredientIndex = ingredientIndex;
            DisplayText = displayText;
            IncludeInTotalTime = includeInTotalTime;
            PortionUsed = portionUsed;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeLinkId { get; private set; }

        public Guid RecipeLinkResourceId { get; private set; }

        public int ParentRecipeId { get; private set; }
        public virtual Recipe ParentRecipe { get; private set; }

        public int LinkedRecipeId { get; private set; }
        public virtual Recipe LinkedRecipe { get; private set; }

        public int? IngredientIndex { get; private set; }

        [StringLength(200)]
        public string DisplayText { get; private set; }

        public bool IncludeInTotalTime { get; private set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? PortionUsed { get; private set; }

        public void Update(int? ingredientIndex, string displayText, bool includeInTotalTime, decimal? portionUsed) {
            IngredientIndex = ingredientIndex;
            DisplayText = displayText;
            IncludeInTotalTime = includeInTotalTime;
            PortionUsed = portionUsed;
        }
    }
}
