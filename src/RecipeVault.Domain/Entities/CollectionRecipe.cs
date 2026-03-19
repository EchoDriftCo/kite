using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeVault.Domain.Entities {
    [Table("CollectionRecipe")]
    public class CollectionRecipe {
        protected CollectionRecipe() {
        }

        public CollectionRecipe(int collectionId, int recipeId, int sortOrder) {
            CollectionId = collectionId;
            RecipeId = recipeId;
            SortOrder = sortOrder;
            AddedDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollectionRecipeId { get; private set; }

        public int CollectionId { get; private set; }

        public int RecipeId { get; private set; }

        public int SortOrder { get; private set; }

        public DateTime AddedDate { get; private set; }

        public virtual Collection Collection { get; private set; }

        public virtual Recipe Recipe { get; private set; }

        public void SetSortOrder(int sortOrder) {
            SortOrder = sortOrder;
        }
    }
}
