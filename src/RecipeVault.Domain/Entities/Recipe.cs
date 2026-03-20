using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.Common.Messages;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.EntityFrameworkCore;
using UUIDNext;

namespace RecipeVault.Domain.Entities {
    [Index(nameof(RecipeResourceId), IsUnique = true)]
    [Index(nameof(ShareToken), IsUnique = true)]
    [Index(nameof(ForkedFromRecipeId))]
    [Table("Recipe")]
    public class Recipe : AuditableEntity {
        protected Recipe() {
        }

        public Recipe(string title, int yield, int? prepTimeMinutes, int? cookTimeMinutes, string description, string source, string originalImageUrl, bool isPublic = false) {
            RecipeResourceId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            ingredients = new List<RecipeIngredient>();
            instructions = new List<RecipeInstruction>();
            recipeTags = new List<RecipeTag>();
            recipeEquipment = new List<RecipeEquipment>();
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

        [StringLength(1000)]
        public string SourceVideoUrl { get; private set; }

        public bool IsPublic { get; private set; }

        public int? Rating { get; private set; }

        public bool IsFavorite { get; private set; }

        public bool IsSampleRecipe { get; private set; }

        [StringLength(50)]
        public string ShowcaseFeature { get; private set; }

        public void MarkAsSample() {
            IsSampleRecipe = true;
        }

        public void SetShowcaseFeature(string feature) {
            ShowcaseFeature = feature;
        }

        [StringLength(12)]
        public string ShareToken { get; private set; }

        // Forking fields
        public int? ForkedFromRecipeId { get; private set; }
        public virtual Recipe ForkedFromRecipe { get; private set; }
        public int ForkCount { get; private set; }

        public void IncrementForkCount() { ForkCount++; }
        
        private readonly List<Recipe> forks = new();
        public virtual IReadOnlyList<Recipe> Forks => forks;

        // Recipe mixing (AI Fusion) fields
        public int? MixedFromRecipeAId { get; private set; }
        public int? MixedFromRecipeBId { get; private set; }
        
        [StringLength(500)]
        public string MixIntent { get; private set; }
        
        public virtual Recipe MixedFromRecipeA { get; private set; }
        public virtual Recipe MixedFromRecipeB { get; private set; }

        private readonly List<RecipeIngredient> ingredients = new();
        public virtual IReadOnlyList<RecipeIngredient> Ingredients => ingredients;

        private readonly List<RecipeInstruction> instructions = new();
        public virtual IReadOnlyList<RecipeInstruction> Instructions => instructions;

        private readonly List<RecipeTag> recipeTags = new();
        public virtual IReadOnlyList<RecipeTag> RecipeTags => recipeTags;

        private readonly List<RecipeEquipment> recipeEquipment = new();
        public virtual IReadOnlyList<RecipeEquipment> RecipeEquipment => recipeEquipment;

        private readonly List<CollectionRecipe> collectionRecipes = new();
        public virtual IReadOnlyList<CollectionRecipe> CollectionRecipes => collectionRecipes;

        private readonly List<RecipeLink> linkedRecipes = new();
        public virtual IReadOnlyList<RecipeLink> LinkedRecipes => linkedRecipes;

        private readonly List<RecipeLink> usedInRecipes = new();
        public virtual IReadOnlyList<RecipeLink> UsedInRecipes => usedInRecipes;

        public void Update(string title, int yield, int? prepTimeMinutes, int? cookTimeMinutes, string description, string source, string originalImageUrl) {
            var messages = new MessageList();
            messages.Aggregate(() => string.IsNullOrWhiteSpace(title), () => new InvalidValueError(nameof(title), title));
            messages.Aggregate(() => title?.Length > 250, () => new InvalidValueError(nameof(title), "Title must not exceed 250 characters"));
            messages.Aggregate(() => yield < 1, () => new InvalidValueError(nameof(yield), yield.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            messages.Aggregate(() => description?.Length > 1000, () => new InvalidValueError(nameof(description), "Description must not exceed 1000 characters"));
            messages.Aggregate(() => source?.Length > 500, () => new InvalidValueError(nameof(source), "Source must not exceed 500 characters"));
            messages.Aggregate(() => originalImageUrl?.Length > 1000, () => new InvalidValueError(nameof(originalImageUrl), "Image URL must not exceed 1000 characters"));
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

        public void SetSourceVideoUrl(string sourceVideoUrl) {
            SourceVideoUrl = sourceVideoUrl;
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

        public void AddEquipment(RecipeEquipment equipment) {
            recipeEquipment.Add(equipment);
        }

        public void RemoveEquipment(RecipeEquipment equipment) {
            recipeEquipment.Remove(equipment);
        }

        public void ClearEquipment() {
            recipeEquipment.Clear();
        }

        public void SetMixedFrom(int recipeAId, int recipeBId, string mixIntent) {
            MixedFromRecipeAId = recipeAId;
            MixedFromRecipeBId = recipeBId;
            MixIntent = mixIntent;
        }

        public Recipe Fork(string newTitle = null) {
            var fork = new Recipe(
                title: newTitle ?? $"{Title} (Copy)",
                yield: Yield,
                prepTimeMinutes: PrepTimeMinutes,
                cookTimeMinutes: CookTimeMinutes,
                description: Description,
                source: null,  // Clear source - this is now user's recipe
                originalImageUrl: OriginalImageUrl,
                isPublic: false  // Forks start private
            );

            // Copy ingredients
            fork.SetIngredients(Ingredients.Select(i => new RecipeIngredient(
                i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText
            )).ToList());

            // Copy instructions
            fork.SetInstructions(Instructions.Select(i => new RecipeInstruction(
                i.StepNumber, i.Instruction, i.RawText
            )).ToList());

            // Link to original
            fork.ForkedFromRecipeId = RecipeId;

            // Copy image URL if exists
            if (!string.IsNullOrEmpty(SourceImageUrl)) {
                fork.SetSourceImageUrl(SourceImageUrl);
            }

            // Copy tags (will be re-assigned to new owner by service)
            foreach (var rt in RecipeTags) {
                fork.AddTag(new RecipeTag(
                    0,  // RecipeId will be set when saved
                    rt.TagId,
                    Guid.Empty,  // SubjectId will be set by service
                    rt.IsAiAssigned,
                    rt.Confidence
                ));
            }

            return fork;
        }
    }
}
