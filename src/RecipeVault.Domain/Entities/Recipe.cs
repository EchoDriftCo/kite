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
    [Index(nameof(ShareToken), IsUnique = true)]
    [Table("Recipe")]
    public class Recipe : AuditableEntity {
        protected Recipe() {
        }

        public Recipe(string title, int yield, int? prepTimeMinutes, int? cookTimeMinutes, string description, string source, string originalImageUrl, bool isPublic = false) {
            RecipeResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            ingredients = new List<RecipeIngredient>();
            instructions = new List<RecipeInstruction>();
            recipeTags = new List<RecipeTag>();
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

        [StringLength(1000)]
        public string SourceImageUrl { get; private set; }

        public bool IsPublic { get; private set; }

        public int? Rating { get; private set; }

        public bool IsFavorite { get; private set; }

        [StringLength(12)]
        public string ShareToken { get; private set; }

        private readonly List<RecipeIngredient> ingredients = new();
        public virtual IReadOnlyList<RecipeIngredient> Ingredients => ingredients;

        private readonly List<RecipeInstruction> instructions = new();
        public virtual IReadOnlyList<RecipeInstruction> Instructions => instructions;

        private readonly List<RecipeTag> recipeTags = new();
        public virtual IReadOnlyList<RecipeTag> RecipeTags => recipeTags;

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

        public void SetSourceImageUrl(string sourceImageUrl) {
            SourceImageUrl = sourceImageUrl;
        }

        public void SetVisibility(bool isPublic) {
            IsPublic = isPublic;
        }

        public void SetRating(int? rating) {
            if (rating.HasValue && (rating.Value < 1 || rating.Value > 5)) {
                var messages = new MessageList();
                messages.Aggregate(() => true, () => new InvalidValueError(nameof(rating), rating.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                messages.ThrowIfAny<ValidationListException>();
            }
            Rating = rating;
        }

        public void SetFavorite(bool isFavorite) {
            IsFavorite = isFavorite;
        }

        public void GenerateShareToken() {
            ShareToken = GenerateRandomToken(10);
        }

        public void RevokeShareToken() {
            ShareToken = null;
        }

        private static string GenerateRandomToken(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++) {
                result[i] = chars[bytes[i] % chars.Length];
            }
            return new string(result);
        }

        public void SetIngredients(List<RecipeIngredient> newIngredients) {
            ingredients.Clear();
            ingredients.AddRange(newIngredients);
        }

        public void SetInstructions(List<RecipeInstruction> newInstructions) {
            instructions.Clear();
            instructions.AddRange(newInstructions);
        }

        public void AddTag(RecipeTag recipeTag) {
            recipeTags.Add(recipeTag);
        }

        public void RemoveTag(RecipeTag recipeTag) {
            recipeTags.Remove(recipeTag);
        }
    }
}
