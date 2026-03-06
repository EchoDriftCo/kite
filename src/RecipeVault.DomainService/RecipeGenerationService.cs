using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cortside.Common.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for AI-powered recipe generation using Gemini
    /// </summary>
    public class RecipeGenerationService : IRecipeGenerationService {
        private readonly IGeminiClient geminiClient;
        private readonly IDietaryProfileService dietaryProfileService;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly IMemoryCache cache;
        private readonly ILogger<RecipeGenerationService> logger;

        private const int MaxGenerationsPerDay = 10;
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromHours(24);
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private static readonly JsonSerializerOptions IndentedJsonOptions = new() { WriteIndented = true };

        private const string SystemPrompt = @"You are a professional chef and recipe developer with expertise in multiple cuisines. 
Create detailed, tested recipes with accurate measurements and clear, step-by-step instructions. 
Focus on practical, home-cookable recipes that real people can successfully prepare.

Generate recipes that are:
- Well-balanced and flavorful
- Achievable for the specified skill level
- Properly portioned with accurate measurements
- Clear and easy to follow
- Respectful of dietary constraints";

        public RecipeGenerationService(
            IGeminiClient geminiClient,
            IDietaryProfileService dietaryProfileService,
            ISubjectPrincipal subjectPrincipal,
            IMemoryCache cache,
            ILogger<RecipeGenerationService> logger) {
            this.geminiClient = geminiClient ?? throw new ArgumentNullException(nameof(geminiClient));
            this.dietaryProfileService = dietaryProfileService ?? throw new ArgumentNullException(nameof(dietaryProfileService));
            this.subjectPrincipal = subjectPrincipal ?? throw new ArgumentNullException(nameof(subjectPrincipal));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        /// <summary>
        /// Generate new recipes based on user prompt and constraints
        /// </summary>
        public async Task<List<GeneratedRecipeDto>> GenerateRecipesAsync(
            GenerateRecipeRequestDto request,
            CancellationToken cancellationToken = default) {

            if (string.IsNullOrWhiteSpace(request.Prompt)) {
                throw new ArgumentException("Prompt is required for recipe generation", nameof(request));
            }

            // Validate variations count
            var variationsCount = Math.Max(1, Math.Min(3, request.Variations));

            // Check rate limit
            await CheckRateLimitAsync(cancellationToken).ConfigureAwait(false);

            // Get user's dietary profile if they have one
            var dietaryConstraints = await GetEffectiveDietaryConstraintsAsync(request.Dietary).ConfigureAwait(false);

            logger.LogInformation(
                "Generating {VariationsCount} recipe(s) for prompt: {Prompt}, constraints: maxTime={MaxTime}, dietary={Dietary}, skill={Skill}",
                variationsCount, request.Prompt, request.MaxTime, string.Join(", ", dietaryConstraints), request.SkillLevel);

            // Build the generation prompt
            var prompt = BuildGenerationPrompt(request.Prompt, request.MaxTime, dietaryConstraints, request.SkillLevel, variationsCount);

            // Call Gemini API
            var responseText = await CallGeminiForRecipeAsync(prompt, cancellationToken).ConfigureAwait(false);

            // Parse response
            var recipes = ParseGeneratedRecipes(responseText, variationsCount);

            // Increment rate limit counter
            IncrementGenerationCount();

            logger.LogInformation("Successfully generated {Count} recipe(s)", recipes.Count);

            return recipes;
        }

        /// <summary>
        /// Refine a previously generated recipe
        /// </summary>
        public async Task<GeneratedRecipeDto> RefineRecipeAsync(
            RefineRecipeRequestDto request,
            CancellationToken cancellationToken = default) {

            if (request?.PreviousRecipe == null) {
                throw new ArgumentException("Previous recipe is required for refinement", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Refinement)) {
                throw new ArgumentException("Refinement instructions are required", nameof(request));
            }

            // Check rate limit
            await CheckRateLimitAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Refining recipe '{Title}' with instructions: {Refinement}",
                request.PreviousRecipe.Title, request.Refinement);

            // Build refinement prompt
            var prompt = BuildRefinementPrompt(request.PreviousRecipe, request.Refinement);

            // Call Gemini API
            var responseText = await CallGeminiForRecipeAsync(prompt, cancellationToken).ConfigureAwait(false);

            // Parse single refined recipe
            var recipes = ParseGeneratedRecipes(responseText, 1);

            // Increment rate limit counter
            IncrementGenerationCount();

            logger.LogInformation("Successfully refined recipe to '{Title}'", recipes[0].Title);

            return recipes[0];
        }

        /// <summary>
        /// Check if user has remaining generation quota
        /// </summary>
        public Task<int> GetRemainingGenerationsAsync() {
            var cacheKey = GetRateLimitCacheKey();
            var count = cache.Get<int>(cacheKey);
            var remaining = Math.Max(0, MaxGenerationsPerDay - count);
            return Task.FromResult(remaining);
        }

        /// <summary>
        /// Get effective dietary constraints by merging request with user's profile
        /// </summary>
        private async Task<List<string>> GetEffectiveDietaryConstraintsAsync(List<string> requestedConstraints) {
            var constraints = new List<string>(requestedConstraints ?? new List<string>());

            try {
                // Try to get user's default dietary profile
                var profiles = await dietaryProfileService.GetProfilesBySubjectAsync().ConfigureAwait(false);
                var defaultProfile = profiles?.FirstOrDefault(p => p.IsDefault);

                if (defaultProfile?.Restrictions != null) {
                    // Add restrictions from profile that aren't already specified
                    foreach (var restriction in defaultProfile.Restrictions) {
                        if (!constraints.Any(c => c.Equals(restriction.RestrictionCode, StringComparison.OrdinalIgnoreCase))) {
                            constraints.Add(restriction.RestrictionCode);
                        }
                    }
                }
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "Could not load dietary profile for recipe generation, continuing without it");
            }

            return constraints;
        }

        /// <summary>
        /// Build the generation prompt for Gemini
        /// </summary>
        private static string BuildGenerationPrompt(
            string userPrompt,
            int? maxTime,
            List<string> dietaryConstraints,
            string skillLevel,
            int variationsCount) {

            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine(SystemPrompt);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"USER REQUEST: {userPrompt}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("CONSTRAINTS:");

            if (maxTime.HasValue) {
                promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"- Maximum total time: {maxTime} minutes (prep + cook)");
            }

            if (dietaryConstraints != null && dietaryConstraints.Count > 0) {
                promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"- Dietary requirements: {string.Join(", ", dietaryConstraints)}");
            }

            if (!string.IsNullOrWhiteSpace(skillLevel)) {
                promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"- Skill level: {skillLevel}");
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"- Number of recipe variations to generate: {variationsCount}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Generate a complete recipe with:");
            promptBuilder.AppendLine("1. Creative but descriptive title");
            promptBuilder.AppendLine("2. Brief description (2-3 sentences)");
            promptBuilder.AppendLine("3. Number of servings (yield)");
            promptBuilder.AppendLine("4. Prep time and cook time in minutes");
            promptBuilder.AppendLine("5. Complete ingredient list with precise measurements");
            promptBuilder.AppendLine("6. Step-by-step instructions");
            promptBuilder.AppendLine("7. Suggested tags (dietary categories, cuisine types, meal types)");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Return ONLY valid JSON.");
            if (variationsCount > 1) {
                promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"Generate exactly {variationsCount} distinct recipe variations and return them as a JSON array of recipe objects.");
            } else {
                promptBuilder.AppendLine("Return a single JSON recipe object.");
            }

            promptBuilder.AppendLine("Recipe object schema:");
            promptBuilder.AppendLine(@"{
  ""title"": ""string"",
  ""description"": ""string"",
  ""yield"": number,
  ""prepTimeMinutes"": number,
  ""cookTimeMinutes"": number,
  ""ingredients"": [
    {
      ""quantity"": number or null,
      ""unit"": ""string or null (cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)"",
      ""item"": ""string (ingredient name)"",
      ""preparation"": ""string or null (e.g., chopped, diced, melted)""
    }
  ],
  ""instructions"": [
    {
      ""stepNumber"": number,
      ""instruction"": ""string""
    }
  ],
  ""tags"": [""string""]
}");

            return promptBuilder.ToString();
        }

        /// <summary>
        /// Build the refinement prompt for Gemini
        /// </summary>
        private static string BuildRefinementPrompt(GeneratedRecipeDto previousRecipe, string refinement) {
            var recipeJson = JsonSerializer.Serialize(new {
                title = previousRecipe.Title,
                description = previousRecipe.Description,
                yield = previousRecipe.Yield,
                prepTimeMinutes = previousRecipe.PrepTimeMinutes,
                cookTimeMinutes = previousRecipe.CookTimeMinutes,
                ingredients = previousRecipe.Ingredients.Select(i => new {
                    quantity = i.Quantity,
                    unit = i.Unit,
                    item = i.Item,
                    preparation = i.Preparation
                }),
                instructions = previousRecipe.Instructions.Select(i => new {
                    stepNumber = i.StepNumber,
                    instruction = i.Instruction
                }),
                tags = previousRecipe.Tags
            }, IndentedJsonOptions);

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine(SystemPrompt);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("PREVIOUS RECIPE:");
            promptBuilder.AppendLine(recipeJson);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine(CultureInfo.InvariantCulture, $"REFINEMENT REQUEST: {refinement}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Generate a refined version of the recipe incorporating the requested changes.");
            promptBuilder.AppendLine("Return the complete refined recipe using the same JSON schema as before.");

            return promptBuilder.ToString();
        }

        /// <summary>
        /// Call Gemini API with recipe generation prompt
        /// </summary>
        private async Task<string> CallGeminiForRecipeAsync(string prompt, CancellationToken cancellationToken) {
            try {
                var responseText = await geminiClient.GenerateTextAsync(prompt, cancellationToken).ConfigureAwait(false);
                return responseText;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error calling Gemini API for recipe generation");
                throw;
            }
        }

        /// <summary>
        /// Parse generated recipe JSON response
        /// </summary>
        private List<GeneratedRecipeDto> ParseGeneratedRecipes(string responseText, int expectedCount) {
            try {
                // Strip markdown code fences if present
                var jsonText = responseText.Trim();
                if (jsonText.StartsWith("```", StringComparison.Ordinal)) {
                    var firstNewline = jsonText.IndexOf('\n');
                    if (firstNewline > 0)
                        jsonText = jsonText.Substring(firstNewline + 1);
                    if (jsonText.EndsWith("```", StringComparison.Ordinal))
                        jsonText = jsonText.Substring(0, jsonText.Length - 3).Trim();
                }

                var recipes = new List<GeneratedRecipeDto>();

                // Try to parse as array first (multiple variations)
                if (jsonText.TrimStart().StartsWith('[')) {
                    recipes = JsonSerializer.Deserialize<List<GeneratedRecipeDto>>(jsonText, JsonOptions);
                } else {
                    // Single recipe
                    var recipe = JsonSerializer.Deserialize<GeneratedRecipeDto>(jsonText, JsonOptions);
                    recipes.Add(recipe);
                }

                return recipes;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Failed to parse generated recipe JSON: {ResponseText}", responseText);
                throw new InvalidOperationException("Failed to parse generated recipe from AI response", ex);
            }
        }

        /// <summary>
        /// Check rate limit and throw if exceeded
        /// </summary>
        private async Task CheckRateLimitAsync(CancellationToken cancellationToken) {
            var remaining = await GetRemainingGenerationsAsync().ConfigureAwait(false);
            if (remaining <= 0) {
                throw new InvalidOperationException(
                    $"Daily generation limit of {MaxGenerationsPerDay} recipes has been reached. Please try again tomorrow.");
            }
        }

        /// <summary>
        /// Increment the generation count for rate limiting
        /// </summary>
        private void IncrementGenerationCount() {
            var cacheKey = GetRateLimitCacheKey();
            var count = cache.GetOrCreate(cacheKey, entry => {
                entry.AbsoluteExpirationRelativeToNow = RateLimitWindow;
                return 0;
            });
            cache.Set(cacheKey, count + 1, RateLimitWindow);
        }

        /// <summary>
        /// Get cache key for rate limiting
        /// </summary>
        private string GetRateLimitCacheKey() {
            return $"recipe_generation_count_{CurrentSubjectId}_{DateTime.UtcNow:yyyy-MM-dd}";
        }
    }
}
