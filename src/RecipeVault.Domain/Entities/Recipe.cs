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
    [Index(nameof(RecipeResourceId), IsUnique = true)]
    [Table("Recipe")]
    public class Recipe : AuditableEntity {
        protected Recipe() {
        }

        public Recipe(string title, int yield, int? prepTimeMinutes, int? cookTimeMinutes, string description, string source, string originalImageUrl, bool isPublic = false) {
            RecipeResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            ingredients = new List<RecipeIngredient>();
            instructions = new List<RecipeInstruction>();
            IsPublic = isPublic;
            Update(title, yield, prepTimeMinutes, cookTimeMinutes, description, source, originalImageUrl);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecipeId { get; private set; }

        public Guid RecipeResourceId { get; private set; }

        [Required]
        [StringLength(250)]
        public string Title { get; private set; }

        [StringLength(1000)]
        public string Description { get; private set; }

        public int Yield { get; private set; }

        public int? PrepTimeMinutes { get; private set; }

        public int? CookTimeMinutes { get; private set; }

        [NotMapped]
        public int? TotalTimeMinutes => PrepTimeMinutes.HasValue && CookTimeMinutes.HasValue
            ? PrepTimeMinutes.Value + CookTimeMinutes.Value
            : null;

        [StringLength(500)]
        public string Source { get; private set; }

        [StringLength(1000)]
        public string OriginalImageUrl { get; private set; }

        public bool IsPublic { get; private set; }

        private readonly List<RecipeIngredient> ingredients = new();
        public virtual IReadOnlyList<RecipeIngredient> Ingredients => ingredients;

        private readonly List<RecipeInstruction> instructions = new();
        public virtual IReadOnlyList<RecipeInstruction> Instructions => instructions;

        public void Update(string title, int yield, int? prepTimeMinutes, int? cookTimeMinutes, string description, string source, string originalImageUrl) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(title), () => new InvalidValueError(nameof(title), title));
            messages.Aggregate(() => yield < 1, () => new InvalidValueError(nameof(yield), yield.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            messages.ThrowIfAny<ValidationListException>();

            Title = title;
            Yield = yield;
            PrepTimeMinutes = prepTimeMinutes;
            CookTimeMinutes = cookTimeMinutes;
            Description = description;
            Source = source;
            OriginalImageUrl = originalImageUrl;
        }

        public void SetVisibility(bool isPublic) {
            IsPublic = isPublic;
        }

        public void SetIngredients(List<RecipeIngredient> newIngredients) {
            ingredients.Clear();
            ingredients.AddRange(newIngredients);
        }

        public void SetInstructions(List<RecipeInstruction> newInstructions) {
            instructions.Clear();
            instructions.AddRange(newInstructions);
        }
    }
}
