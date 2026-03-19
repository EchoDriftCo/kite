using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(CollectionResourceId), IsUnique = true)]
    [Table("Collection")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Collection is the correct domain term for this entity")]
    public class Collection : AuditableEntity {
        protected Collection() {
        }

        public Collection(string name, string description, Guid subjectId) {
            CollectionResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            collectionRecipes = new List<CollectionRecipe>();
            SubjectId = subjectId;
            IsPublic = false;
            IsFeatured = false;
            SortOrder = 0;
            Update(name, description);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollectionId { get; private set; }

        public Guid CollectionResourceId { get; private set; }

        public Guid SubjectId { get; private set; }

        [Required]
        [StringLength(100)]
        public string Name { get; private set; }

        [StringLength(500)]
        public string Description { get; private set; }

        [StringLength(1000)]
        public string CoverImageUrl { get; private set; }

        public bool IsPublic { get; private set; }

        public bool IsFeatured { get; private set; }

        public int SortOrder { get; private set; }

        private readonly List<CollectionRecipe> collectionRecipes = new();
        public virtual IReadOnlyList<CollectionRecipe> CollectionRecipes => collectionRecipes;

        public void Update(string name, string description) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(name), () => new InvalidValueError(nameof(name), name));
            messages.ThrowIfAny<ValidationListException>();

            Name = name;
            Description = description;
        }

        public void SetCoverImage(string coverImageUrl) {
            CoverImageUrl = coverImageUrl;
        }

        public void SetVisibility(bool isPublic) {
            IsPublic = isPublic;
        }

        public void SetFeatured(bool isFeatured) {
            IsFeatured = isFeatured;
        }

        public void SetSortOrder(int sortOrder) {
            SortOrder = sortOrder;
        }

        public CollectionRecipe AddRecipe(int recipeId, int sortOrder) {
            // Check if recipe is already in collection
            if (collectionRecipes.Exists(cr => cr.RecipeId == recipeId)) {
                throw new InvalidOperationException("Recipe is already in this collection");
            }

            var collectionRecipe = new CollectionRecipe(CollectionId, recipeId, sortOrder);
            collectionRecipes.Add(collectionRecipe);
            return collectionRecipe;
        }

        public void RemoveRecipe(CollectionRecipe collectionRecipe) {
            collectionRecipes.Remove(collectionRecipe);
        }

        public void ReorderRecipes(Dictionary<int, int> recipeIdToSortOrder) {
            foreach (var cr in collectionRecipes) {
                if (recipeIdToSortOrder.TryGetValue(cr.RecipeId, out var sortOrder)) {
                    cr.SetSortOrder(sortOrder);
                }
            }
        }
    }
}
