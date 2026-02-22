using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cortside.Common.Logging;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService.Models;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public class ImportService : IImportService {
        private readonly ILogger<ImportService> logger;
        private readonly IRecipeRepository recipeRepository;
        private readonly ITagService tagService;
        private readonly IImageStorage imageStorage;

        public ImportService(
            IRecipeRepository recipeRepository,
            ITagService tagService,
            IImageStorage imageStorage,
            ILogger<ImportService> logger) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
            this.tagService = tagService;
            this.imageStorage = imageStorage;
        }

        public async Task<ImportResultDto> ImportFromPaprikaAsync(Stream fileStream) {
            var result = new ImportResultDto();

            try {
                // Decompress the gzipped file
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);

                // Parse JSON array of recipes
                var paprikaRecipes = JsonSerializer.Deserialize<List<PaprikaRecipe>>(json);

                if (paprikaRecipes == null || paprikaRecipes.Count == 0) {
                    logger.LogWarning("No recipes found in Paprika file");
                    return result;
                }

                result.TotalRecipes = paprikaRecipes.Count;
                logger.LogInformation("Found {RecipeCount} recipes in Paprika file", paprikaRecipes.Count);

                // Import each recipe
                foreach (var paprikaRecipe in paprikaRecipes) {
                    try {
                        var recipe = await ImportSingleRecipeAsync(paprikaRecipe).ConfigureAwait(false);
                        result.ImportedRecipes.Add(new ImportedRecipeDto {
                            RecipeResourceId = recipe.RecipeResourceId,
                            Title = recipe.Title
                        });
                        result.SuccessCount++;
                    }
                    catch (Exception ex) {
                        logger.LogError(ex, "Failed to import recipe: {RecipeName}", paprikaRecipe?.Name ?? "Unknown");
                        result.Errors.Add(new ImportErrorDto {
                            RecipeName = paprikaRecipe?.Name ?? "Unknown",
                            ErrorMessage = ex.Message
                        });
                        result.FailureCount++;
                    }
                }

                logger.LogInformation("Paprika import completed. Success: {SuccessCount}, Failures: {FailureCount}",
                    result.SuccessCount, result.FailureCount);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Failed to process Paprika file");
                throw;
            }

            return result;
        }

        private async Task<Recipe> ImportSingleRecipeAsync(PaprikaRecipe paprikaRecipe) {
            using (logger.PushProperty("PaprikaRecipeName", paprikaRecipe.Name)) {
                // Map to RecipeVault recipe
                var recipe = new Recipe(
                    title: paprikaRecipe.Name,
                    yield: ParseServings(paprikaRecipe.Servings),
                    prepTimeMinutes: ParseTimeMinutes(paprikaRecipe.PrepTime),
                    cookTimeMinutes: ParseTimeMinutes(paprikaRecipe.CookTime),
                    description: paprikaRecipe.Notes,
                    source: !string.IsNullOrWhiteSpace(paprikaRecipe.Source) ? paprikaRecipe.Source : paprikaRecipe.SourceUrl,
                    originalImageUrl: null,  // Will handle photo_data separately
                    isPublic: false  // Imported recipes start private
                );

                // Parse ingredients
                var ingredients = ParseIngredients(paprikaRecipe.Ingredients);
                recipe.SetIngredients(ingredients);

                // Parse instructions
                var instructions = ParseInstructions(paprikaRecipe.Directions);
                recipe.SetInstructions(instructions);

                // Handle photo_data (base64 encoded image)
                if (!string.IsNullOrWhiteSpace(paprikaRecipe.PhotoData)) {
                    try {
                        var imageUrl = await UploadBase64ImageAsync(paprikaRecipe.PhotoData, paprikaRecipe.Name).ConfigureAwait(false);
                        recipe.SetSourceImageUrl(imageUrl);
                    }
                    catch (Exception ex) {
                        logger.LogWarning(ex, "Failed to upload image for recipe: {RecipeName}", paprikaRecipe.Name);
                        // Continue without image - not a fatal error
                    }
                }

                // Save recipe
                await recipeRepository.AddAsync(recipe).ConfigureAwait(false);

                // Handle categories as tags
                if (paprikaRecipe.Categories != null && paprikaRecipe.Categories.Length > 0) {
                    await AssignCategoryTagsAsync(recipe, paprikaRecipe.Categories).ConfigureAwait(false);
                }

                logger.LogInformation("Successfully imported recipe: {RecipeName}", paprikaRecipe.Name);
                return recipe;
            }
        }

        private static int ParseServings(string servings) {
            if (string.IsNullOrWhiteSpace(servings)) {
                return 4; // Default servings
            }

            // Try to extract number from string like "4", "4 servings", "Serves 4", etc.
            var match = Regex.Match(servings, @"\d+");
            if (match.Success && int.TryParse(match.Value, out var result)) {
                return result > 0 ? result : 4;
            }

            return 4; // Default if parsing fails
        }

        private static int? ParseTimeMinutes(string timeString) {
            if (string.IsNullOrWhiteSpace(timeString)) {
                return null;
            }

            // Handle formats like: "30 minutes", "1 hour", "1 hour 30 minutes", "90 min", etc.
            var totalMinutes = 0;

            // Extract hours
            var hoursMatch = Regex.Match(timeString, @"(\d+)\s*(hour|hr|h)", RegexOptions.IgnoreCase);
            if (hoursMatch.Success) {
                if (int.TryParse(hoursMatch.Groups[1].Value, out var hours)) {
                    totalMinutes += hours * 60;
                }
            }

            // Extract minutes
            var minutesMatch = Regex.Match(timeString, @"(\d+)\s*(minute|min|m)(?!onth)", RegexOptions.IgnoreCase);
            if (minutesMatch.Success) {
                if (int.TryParse(minutesMatch.Groups[1].Value, out var minutes)) {
                    totalMinutes += minutes;
                }
            }

            // If no hours or minutes found, try to parse as plain number
            if (totalMinutes == 0) {
                var numberMatch = Regex.Match(timeString, @"^\d+$");
                if (numberMatch.Success && int.TryParse(numberMatch.Value, out var plainMinutes)) {
                    totalMinutes = plainMinutes;
                }
            }

            return totalMinutes > 0 ? totalMinutes : null;
        }

        private static readonly char[] LineSeparators = ['\r', '\n'];

        private static List<RecipeIngredient> ParseIngredients(string ingredientsText) {
            if (string.IsNullOrWhiteSpace(ingredientsText)) {
                return new List<RecipeIngredient>();
            }

            // Split by newlines and filter out empty lines
            var lines = ingredientsText
                .Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var ingredients = new List<RecipeIngredient>();
            for (int i = 0; i < lines.Count; i++) {
                // For imported recipes, we store the raw text as-is
                // The RecipeIngredient constructor can parse it if needed
                ingredients.Add(new RecipeIngredient(
                    sortOrder: i + 1,
                    quantity: null,  // Could enhance with parsing
                    unit: null,
                    item: null,
                    preparation: null,
                    rawText: lines[i]
                ));
            }

            return ingredients;
        }

        private static List<RecipeInstruction> ParseInstructions(string directionsText) {
            if (string.IsNullOrWhiteSpace(directionsText)) {
                return new List<RecipeInstruction>();
            }

            // Split by newlines and filter out empty lines
            var lines = directionsText
                .Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var instructions = new List<RecipeInstruction>();
            for (int i = 0; i < lines.Count; i++) {
                instructions.Add(new RecipeInstruction(
                    stepNumber: i + 1,
                    instruction: lines[i],
                    rawText: lines[i]
                ));
            }

            return instructions;
        }

        private async Task<string> UploadBase64ImageAsync(string base64Data, string recipeName) {
            // Remove data URI prefix if present (e.g., "data:image/jpeg;base64,")
            var base64 = base64Data;
            if (base64Data.Contains(',')) {
                base64 = base64Data.Substring(base64Data.IndexOf(',') + 1);
            }

            // Convert base64 to bytes
            var imageBytes = Convert.FromBase64String(base64);

            // Generate filename from recipe name
            var fileName = $"{SanitizeFileName(recipeName)}_{Guid.NewGuid()}.jpg";

            // Upload to storage
            var imageUrl = await imageStorage.UploadAsync(imageBytes, fileName, "image/jpeg").ConfigureAwait(false);

            return imageUrl;
        }

        private static string SanitizeFileName(string fileName) {
            // Remove invalid filename characters
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            
            // Limit length
            if (sanitized.Length > 50) {
                sanitized = sanitized.Substring(0, 50);
            }

            return sanitized;
        }

        private async Task AssignCategoryTagsAsync(Recipe recipe, string[] categories) {
            foreach (var category in categories) {
                if (string.IsNullOrWhiteSpace(category)) {
                    continue;
                }

                try {
                    // Get or create tag with "Custom" tag category
                    var tag = await tagService.GetOrCreateTagAsync(category.Trim(), TagCategory.Custom).ConfigureAwait(false);

                    // Create RecipeTag association
                    var recipeTag = new RecipeTag(
                        recipeId: recipe.RecipeId,
                        tagId: tag.TagId,
                        assignedBySubjectId: Guid.Empty,  // System/import assigned
                        isAiAssigned: false,
                        confidence: null
                    );

                    recipe.AddTag(recipeTag);
                }
                catch (Exception ex) {
                    logger.LogWarning(ex, "Failed to assign category tag: {Category}", category);
                    // Continue with other tags
                }
            }
        }
    }
}
