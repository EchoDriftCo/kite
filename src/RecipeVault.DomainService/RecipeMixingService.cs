using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService.Models;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for mixing two recipes together using AI
    /// </summary>
    public class RecipeMixingService : IRecipeMixingService {
        private readonly IGeminiClient geminiClient;
        private readonly ILogger<RecipeMixingService> logger;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions JsonIndentedOptions = new JsonSerializerOptions { WriteIndented = true };

        public RecipeMixingService(IGeminiClient geminiClient, ILogger<RecipeMixingService> logger) {
            this.geminiClient = geminiClient;
            this.logger = logger;
        }

        public async Task<MixedRecipePreview> MixRecipesAsync(
            Recipe recipeA,
            Recipe recipeB,
            string intent,
            string mode,
            CancellationToken cancellationToken = default) {

            logger.LogInformation("Mixing recipes: {RecipeA} + {RecipeB}, mode={Mode}", recipeA.Title, recipeB.Title, mode);

            var prompt = BuildMixingPrompt(recipeA, recipeB, intent, mode);
            var responseText = await CallGeminiForMixingAsync(prompt, cancellationToken);

            var preview = ParseMixedRecipeResponse(responseText, recipeA, recipeB);
            return preview;
        }

        public async Task<MixedRecipePreview> RefineMixedRecipeAsync(
            MixedRecipePreview preview,
            string refinementNotes,
            CancellationToken cancellationToken = default) {

            logger.LogInformation("Refining mixed recipe: {Title}", preview.Title);

            var prompt = BuildRefinementPrompt(preview, refinementNotes);
            var responseText = await CallGeminiForMixingAsync(prompt, cancellationToken);

            var refinedPreview = ParseMixedRecipeResponse(responseText, null, null);
            return refinedPreview;
        }

        private static string BuildMixingPrompt(Recipe recipeA, Recipe recipeB, string intent, string mode) {
            var sb = new StringBuilder();
            sb.AppendLine("You are a creative culinary AI tasked with mixing two recipes into one new recipe.");
            sb.AppendLine();

            // Describe the mode
            switch (mode?.ToLowerInvariant()) {
                case "guided":
                    sb.AppendLine(CultureInfo.InvariantCulture, $"MODE: Guided - The user has specified this intent: \"{intent}\"");
                    sb.AppendLine("Create a recipe that fulfills this intent by thoughtfully combining elements from both recipes.");
                    break;
                case "surprise":
                    sb.AppendLine("MODE: Surprise Me - Be creative and unexpected! Mix these recipes in a way the user wouldn't predict.");
                    sb.AppendLine("Think outside the box and create something innovative.");
                    break;
                case "bestofboth":
                    sb.AppendLine("MODE: Best of Both - Combine the best elements of each recipe into a superior final dish.");
                    sb.AppendLine("Keep what makes each recipe great, and harmonize them together.");
                    break;
                default:
                    sb.AppendLine("MODE: Best of Both - Combine the best elements of each recipe.");
                    break;
            }

            sb.AppendLine();
            sb.AppendLine("=== RECIPE A ===");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Title: {recipeA.Title}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Description: {recipeA.Description}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Yield: {recipeA.Yield} servings");
            if (recipeA.PrepTimeMinutes.HasValue)
                sb.AppendLine(CultureInfo.InvariantCulture, $"Prep Time: {recipeA.PrepTimeMinutes} minutes");
            if (recipeA.CookTimeMinutes.HasValue)
                sb.AppendLine(CultureInfo.InvariantCulture, $"Cook Time: {recipeA.CookTimeMinutes} minutes");
            sb.AppendLine();
            sb.AppendLine("Ingredients:");
            foreach (var ing in recipeA.Ingredients) {
                sb.AppendLine($"- {ing.Quantity} {ing.Unit} {ing.Item} {ing.Preparation}".Trim());
            }
            sb.AppendLine();
            sb.AppendLine("Instructions:");
            foreach (var inst in recipeA.Instructions) {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{inst.StepNumber}. {inst.Instruction}");
            }

            sb.AppendLine();
            sb.AppendLine("=== RECIPE B ===");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Title: {recipeB.Title}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Description: {recipeB.Description}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Yield: {recipeB.Yield} servings");
            if (recipeB.PrepTimeMinutes.HasValue)
                sb.AppendLine(CultureInfo.InvariantCulture, $"Prep Time: {recipeB.PrepTimeMinutes} minutes");
            if (recipeB.CookTimeMinutes.HasValue)
                sb.AppendLine(CultureInfo.InvariantCulture, $"Cook Time: {recipeB.CookTimeMinutes} minutes");
            sb.AppendLine();
            sb.AppendLine("Ingredients:");
            foreach (var ing in recipeB.Ingredients) {
                sb.AppendLine($"- {ing.Quantity} {ing.Unit} {ing.Item} {ing.Preparation}".Trim());
            }
            sb.AppendLine();
            sb.AppendLine("Instructions:");
            foreach (var inst in recipeB.Instructions) {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{inst.StepNumber}. {inst.Instruction}");
            }

            sb.AppendLine();
            sb.AppendLine("=== YOUR TASK ===");
            sb.AppendLine("Create a new mixed recipe. Output JSON with this exact structure:");
            sb.AppendLine(@"{
  ""title"": ""Mixed Recipe Title"",
  ""description"": ""Brief description of the mixed recipe"",
  ""yield"": 4,
  ""prepTimeMinutes"": 20,
  ""cookTimeMinutes"": 30,
  ""ingredients"": [
    {
      ""quantity"": 1.5,
      ""unit"": ""cup"",
      ""item"": ""ingredient name"",
      ""preparation"": ""chopped"",
      ""rawText"": ""1.5 cups ingredient name, chopped"",
      ""attribution"": ""from A"" // or ""from B"" or ""combined""
    }
  ],
  ""instructions"": [
    {
      ""stepNumber"": 1,
      ""instruction"": ""Step instruction text"",
      ""rawText"": ""Step instruction text"",
      ""attribution"": ""from A"" // or ""from B"" or ""combined""
    }
  ],
  ""mixNotes"": ""Explain how you mixed the recipes and what elements came from each.""
}");

            sb.AppendLine();
            sb.AppendLine("Output ONLY valid JSON. No markdown, no code fences, no explanations outside the JSON.");

            return sb.ToString();
        }

        private static string BuildRefinementPrompt(MixedRecipePreview preview, string refinementNotes) {
            var sb = new StringBuilder();
            sb.AppendLine("You are refining a mixed recipe based on user feedback.");
            sb.AppendLine();
            sb.AppendLine("=== CURRENT RECIPE ===");
            sb.AppendLine(JsonSerializer.Serialize(preview, JsonIndentedOptions));
            sb.AppendLine();
            sb.AppendLine("=== USER REFINEMENT NOTES ===");
            sb.AppendLine(refinementNotes);
            sb.AppendLine();
            sb.AppendLine("=== YOUR TASK ===");
            sb.AppendLine("Modify the recipe according to the user's notes. Output JSON in the same format as above.");
            sb.AppendLine("Output ONLY valid JSON. No markdown, no code fences, no explanations outside the JSON.");

            return sb.ToString();
        }

        private async Task<string> CallGeminiForMixingAsync(string prompt, CancellationToken cancellationToken) {
            return await geminiClient.GenerateTextAsync(prompt, "application/json", cancellationToken);
        }

        private MixedRecipePreview ParseMixedRecipeResponse(string responseText, Recipe recipeA, Recipe recipeB) {
            try {
                // Clean up the response (remove markdown code fences if present)
                var jsonText = responseText.Trim();
                if (jsonText.StartsWith("```json", StringComparison.Ordinal)) {
                    jsonText = jsonText.Substring(7);
                }
                if (jsonText.StartsWith("```", StringComparison.Ordinal)) {
                    jsonText = jsonText.Substring(3);
                }
                if (jsonText.EndsWith("```", StringComparison.Ordinal)) {
                    jsonText = jsonText.Substring(0, jsonText.Length - 3);
                }
                jsonText = jsonText.Trim();

                var preview = JsonSerializer.Deserialize<MixedRecipePreview>(jsonText, JsonOptions);

                if (recipeA != null && recipeB != null) {
                    preview.Source = $"Mixed from: {recipeA.Title} + {recipeB.Title}";
                }

                return preview;
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to parse mixed recipe response");
                throw new InvalidOperationException("Failed to parse AI response", ex);
            }
        }
    }
}
