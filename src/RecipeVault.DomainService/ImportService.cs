using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cortside.Common.Logging;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.DomainService.Models;
using RecipeVault.DomainService.Utilities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using Cortside.Common.Messages.MessageExceptions;
using RecipeVault.Integrations.Gemini;

namespace RecipeVault.DomainService {
    public class ImportService : IImportService {
        private readonly ILogger<ImportService> logger;
        private readonly IRecipeRepository recipeRepository;
        private readonly ITagService tagService;
        private readonly IImageStorage imageStorage;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IGeminiClient geminiClient;

        public ImportService(
            IRecipeRepository recipeRepository,
            ITagService tagService,
            IImageStorage imageStorage,
            IHttpClientFactory httpClientFactory,
            IGeminiClient geminiClient,
            ILogger<ImportService> logger) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
            this.tagService = tagService;
            this.imageStorage = imageStorage;
            this.httpClientFactory = httpClientFactory;
            this.geminiClient = geminiClient;
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

        public async Task<Recipe> ImportFromUrlAsync(string url) {
            using (logger.PushProperty("ImportUrl", url)) {
                logger.LogInformation("Starting recipe import from URL: {Url}", url);

                // Fetch HTML from URL
                var html = await FetchHtmlAsync(url).ConfigureAwait(false);

                // Try to extract schema.org/Recipe JSON-LD first
                var schemaRecipe = ExtractSchemaOrgRecipe(html);

                Recipe recipe;
                if (schemaRecipe != null) {
                    logger.LogInformation("Found schema.org/Recipe markup, parsing structured data");
                    recipe = MapSchemaOrgToRecipe(schemaRecipe, url);
                } else {
                    logger.LogInformation("No schema.org markup found, falling back to Gemini AI parsing");
                    recipe = await ParseWithGeminiAsync(html, url).ConfigureAwait(false);
                }

                // Save recipe
                await recipeRepository.AddAsync(recipe).ConfigureAwait(false);

                logger.LogInformation("Successfully imported recipe from URL: {Title}", recipe.Title);
                return recipe;
            }
        }

        private async Task<string> FetchHtmlAsync(string url) {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
                throw new ArgumentException("Invalid URL format", nameof(url));
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) {
                throw new ArgumentException("URL must use HTTP or HTTPS protocol", nameof(url));
            }

            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "RecipeVault/1.0 (Recipe Importer)");

            try {
                var response = await httpClient.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return html;
            }
            catch (HttpRequestException ex) {
                logger.LogError(ex, "Failed to fetch URL: {Url}", url);
                throw new InvalidOperationException($"Failed to fetch recipe from URL: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) {
                logger.LogError(ex, "Request timeout for URL: {Url}", url);
                throw new InvalidOperationException("Request timed out while fetching recipe", ex);
            }
        }

        private SchemaOrgRecipe ExtractSchemaOrgRecipe(string html) {
            // Look for JSON-LD script tags with @type Recipe
            var jsonLdPattern = @"<script[^>]*type\s*=\s*[""']application/ld\+json[""'][^>]*>(.*?)</script>";
            var matches = Regex.Matches(html, jsonLdPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in matches) {
                try {
                    var jsonContent = match.Groups[1].Value.Trim();
                    
                    // Try to parse as single object
                    using var doc = JsonDocument.Parse(jsonContent);
                    var root = doc.RootElement;

                    // Handle both single object and array of objects
                    if (root.ValueKind == JsonValueKind.Array) {
                        foreach (var element in root.EnumerateArray()) {
                            var recipe = TryParseSchemaOrgRecipe(element);
                            if (recipe != null) {
                                return recipe;
                            }
                        }
                    } else {
                        var recipe = TryParseSchemaOrgRecipe(root);
                        if (recipe != null) {
                            return recipe;
                        }
                    }
                }
                catch (JsonException ex) {
                    logger.LogDebug(ex, "Failed to parse JSON-LD block, trying next one");
                    // Continue to next script tag
                }
            }

            return null;
        }

        private SchemaOrgRecipe TryParseSchemaOrgRecipe(JsonElement element) {
            // Check if this is a Recipe type
            if (element.TryGetProperty("@type", out var typeProperty)) {
                var type = typeProperty.GetString();
                if (type != null && type.Equals("Recipe", StringComparison.OrdinalIgnoreCase)) {
                    try {
                        return JsonSerializer.Deserialize<SchemaOrgRecipe>(element.GetRawText());
                    }
                    catch (JsonException ex) {
                        logger.LogWarning(ex, "Found Recipe type but failed to deserialize");
                    }
                }
            }

            return null;
        }

        private static Recipe MapSchemaOrgToRecipe(SchemaOrgRecipe schemaRecipe, string sourceUrl) {
            var title = schemaRecipe.Name ?? "Imported Recipe";
            var yield = schemaRecipe.RecipeYield ?? 4;
            var prepTime = Iso8601DurationParser.ParseToMinutes(schemaRecipe.PrepTime);
            var cookTime = Iso8601DurationParser.ParseToMinutes(schemaRecipe.CookTime);

            // If only totalTime is provided and not prep/cook, use it as cookTime
            if (!prepTime.HasValue && !cookTime.HasValue && !string.IsNullOrWhiteSpace(schemaRecipe.TotalTime)) {
                cookTime = Iso8601DurationParser.ParseToMinutes(schemaRecipe.TotalTime);
            }

            var description = schemaRecipe.Description;
            var source = schemaRecipe.Author ?? sourceUrl;
            var imageUrl = schemaRecipe.Image;

            var recipe = new Recipe(
                title: title,
                yield: yield,
                prepTimeMinutes: prepTime,
                cookTimeMinutes: cookTime,
                description: description,
                source: source,
                originalImageUrl: imageUrl,
                isPublic: false
            );

            // Map ingredients
            if (schemaRecipe.RecipeIngredient != null && schemaRecipe.RecipeIngredient.Length > 0) {
                var ingredients = new List<RecipeIngredient>();
                for (int i = 0; i < schemaRecipe.RecipeIngredient.Length; i++) {
                    ingredients.Add(new RecipeIngredient(
                        sortOrder: i + 1,
                        quantity: null,
                        unit: null,
                        item: null,
                        preparation: null,
                        rawText: schemaRecipe.RecipeIngredient[i]
                    ));
                }
                recipe.SetIngredients(ingredients);
            }

            // Map instructions
            if (schemaRecipe.RecipeInstructions != null && schemaRecipe.RecipeInstructions.Length > 0) {
                var instructions = new List<RecipeInstruction>();
                for (int i = 0; i < schemaRecipe.RecipeInstructions.Length; i++) {
                    instructions.Add(new RecipeInstruction(
                        stepNumber: i + 1,
                        instruction: schemaRecipe.RecipeInstructions[i],
                        rawText: schemaRecipe.RecipeInstructions[i]
                    ));
                }
                recipe.SetInstructions(instructions);
            }

            return recipe;
        }

        private async Task<Recipe> ParseWithGeminiAsync(string html, string sourceUrl) {
            try {
                // Strip HTML tags but preserve basic structure (newlines)
                var textContent = StripHtmlTags(html);

                var geminiResponse = await geminiClient.ParseRecipeTextAsync(textContent).ConfigureAwait(false);

                var title = geminiResponse.Title ?? "Imported Recipe";
                var yield = geminiResponse.Yield ?? 4;
                var prepTime = geminiResponse.PrepTimeMinutes;
                var cookTime = geminiResponse.CookTimeMinutes;

                var recipe = new Recipe(
                    title: title,
                    yield: yield,
                    prepTimeMinutes: prepTime,
                    cookTimeMinutes: cookTime,
                    description: null,
                    source: sourceUrl,
                    originalImageUrl: null,
                    isPublic: false
                );

                // Map Gemini ingredients
                if (geminiResponse.Ingredients != null && geminiResponse.Ingredients.Count > 0) {
                    var ingredients = new List<RecipeIngredient>();
                    for (int i = 0; i < geminiResponse.Ingredients.Count; i++) {
                        var geminiIngredient = geminiResponse.Ingredients[i];
                        ingredients.Add(new RecipeIngredient(
                            sortOrder: i + 1,
                            quantity: geminiIngredient.Quantity,
                            unit: geminiIngredient.Unit,
                            item: geminiIngredient.Item,
                            preparation: geminiIngredient.Preparation,
                            rawText: geminiIngredient.RawText ?? $"{geminiIngredient.Quantity} {geminiIngredient.Unit} {geminiIngredient.Item} {geminiIngredient.Preparation}".Trim()
                        ));
                    }
                    recipe.SetIngredients(ingredients);
                }

                // Map Gemini instructions
                if (geminiResponse.Instructions != null && geminiResponse.Instructions.Count > 0) {
                    var instructions = new List<RecipeInstruction>();
                    foreach (var geminiInstruction in geminiResponse.Instructions) {
                        instructions.Add(new RecipeInstruction(
                            stepNumber: geminiInstruction.StepNumber,
                            instruction: geminiInstruction.Instruction,
                            rawText: geminiInstruction.RawText ?? geminiInstruction.Instruction
                        ));
                    }
                    recipe.SetInstructions(instructions);
                }

                logger.LogInformation("Gemini parsing completed with confidence: {Confidence}", geminiResponse.Confidence);
                return recipe;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Gemini parsing failed for URL: {Url}", sourceUrl);
                throw new InvalidOperationException("Failed to parse recipe with AI: " + ex.Message, ex);
            }
        }

        public async Task<Recipe> ImportFromMultipleImagesAsync(List<Stream> imageStreams, string processingMode = "sequential") {
            using (logger.PushProperty("ImageCount", imageStreams.Count))
            using (logger.PushProperty("ProcessingMode", processingMode)) {
                logger.LogInformation("Starting multi-image recipe import with {ImageCount} images", imageStreams.Count);

                if (imageStreams == null || imageStreams.Count == 0) {
                    throw new ArgumentException("At least one image is required", nameof(imageStreams));
                }

                if (imageStreams.Count > 4) {
                    throw new ArgumentException("Maximum 4 images allowed", nameof(imageStreams));
                }

                // Sequential processing: parse each image and combine results
                if (processingMode == "sequential") {
                    return await ProcessImagesSequentiallyAsync(imageStreams).ConfigureAwait(false);
                } else {
                    throw new NotImplementedException("Only 'sequential' processing mode is currently supported");
                }
            }
        }

        private async Task<Recipe> ProcessImagesSequentiallyAsync(List<Stream> imageStreams) {
            var allIngredients = new List<GeminiIngredient>();
            var allInstructions = new List<GeminiInstruction>();
            string title = null;
            int? yield = null;
            int? prepTime = null;
            int? cookTime = null;

            int imageIndex = 1;
            foreach (var stream in imageStreams) {
                logger.LogInformation("Processing image {Index} of {Total}", imageIndex, imageStreams.Count);
                
                // Convert stream to base64
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                // Detect MIME type (simple detection based on magic bytes)
                var mimeType = DetectImageMimeType(imageBytes);

                // Parse image with Gemini
                var geminiResponse = await geminiClient.ParseRecipeAsync(base64Image, mimeType).ConfigureAwait(false);

                // Use title from first image, or first non-null title
                if (title == null && !string.IsNullOrWhiteSpace(geminiResponse.Title)) {
                    title = geminiResponse.Title;
                }

                // Use yield from first image that has it
                if (!yield.HasValue && geminiResponse.Yield.HasValue) {
                    yield = geminiResponse.Yield;
                }

                // Accumulate times (use maximum if multiple images have times)
                if (geminiResponse.PrepTimeMinutes.HasValue) {
                    prepTime = Math.Max(prepTime ?? 0, geminiResponse.PrepTimeMinutes.Value);
                }
                if (geminiResponse.CookTimeMinutes.HasValue) {
                    cookTime = Math.Max(cookTime ?? 0, geminiResponse.CookTimeMinutes.Value);
                }

                // Collect ingredients
                if (geminiResponse.Ingredients != null && geminiResponse.Ingredients.Count > 0) {
                    allIngredients.AddRange(geminiResponse.Ingredients);
                }

                // Collect instructions, adjusting step numbers for continuation
                if (geminiResponse.Instructions != null && geminiResponse.Instructions.Count > 0) {
                    var stepOffset = allInstructions.Count;
                    foreach (var instruction in geminiResponse.Instructions) {
                        allInstructions.Add(new GeminiInstruction {
                            StepNumber = stepOffset + instruction.StepNumber,
                            Instruction = instruction.Instruction,
                            RawText = instruction.RawText
                        });
                    }
                }

                imageIndex++;
            }

            // Log what Gemini returned
            logger.LogInformation("Multi-image parsing complete: {IngredientCount} ingredients, {InstructionCount} instructions",
                allIngredients.Count, allInstructions.Count);

            // Validate that we got meaningful content
            if (allIngredients.Count == 0 && allInstructions.Count == 0) {
                logger.LogError("Gemini returned ZERO ingredients AND ZERO instructions from {ImageCount} images - cannot create empty recipe",
                    imageStreams.Count);
                throw new InvalidOperationException(
                    $"Could not extract recipe data from {imageStreams.Count} image(s). " +
                    "The images may not contain a recipe, or the recipe text may not be readable. " +
                    "Please ensure images are clear, well-lit, and contain visible recipe text.");
            }

            if (allIngredients.Count == 0) {
                logger.LogWarning("Gemini returned ZERO ingredients from {ImageCount} images - recipe will have no ingredients",
                    imageStreams.Count);
            }

            if (allInstructions.Count == 0) {
                logger.LogWarning("Gemini returned ZERO instructions from {ImageCount} images - recipe will have no instructions",
                    imageStreams.Count);
            }

            // Create recipe from combined results
            var recipe = new Recipe(
                title: title ?? "Multi-Image Recipe Import",
                yield: yield ?? 4,
                prepTimeMinutes: prepTime,
                cookTimeMinutes: cookTime,
                description: $"Imported from {imageStreams.Count} image(s)",
                source: "Multi-Image Import",
                originalImageUrl: null,
                isPublic: false
            );

            // Set combined ingredients
            var ingredients = new List<RecipeIngredient>();
            for (int i = 0; i < allIngredients.Count; i++) {
                var geminiIng = allIngredients[i];
                ingredients.Add(new RecipeIngredient(
                    sortOrder: i + 1,
                    quantity: geminiIng.Quantity,
                    unit: geminiIng.Unit,
                    item: geminiIng.Item,
                    preparation: geminiIng.Preparation,
                    rawText: geminiIng.RawText
                ));
            }
            recipe.SetIngredients(ingredients);

            // Set combined instructions
            var instructions = new List<RecipeInstruction>();
            for (int i = 0; i < allInstructions.Count; i++) {
                var geminiInst = allInstructions[i];
                instructions.Add(new RecipeInstruction(
                    stepNumber: i + 1,
                    instruction: geminiInst.Instruction,
                    rawText: geminiInst.RawText ?? geminiInst.Instruction
                ));
            }
            recipe.SetInstructions(instructions);

            logger.LogInformation("Recipe built with {IngredientCount} ingredients and {InstructionCount} instructions",
                recipe.Ingredients.Count, recipe.Instructions.Count);

            // Save recipe
            await recipeRepository.AddAsync(recipe).ConfigureAwait(false);

            logger.LogInformation("Successfully imported multi-image recipe: {Title} ({IngredientCount} ingredients, {InstructionCount} steps)",
                recipe.Title, ingredients.Count, instructions.Count);

            return recipe;
        }

        private static string DetectImageMimeType(byte[] imageBytes) {
            if (imageBytes.Length < 4) {
                return "image/jpeg"; // Default
            }

            // PNG signature
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47) {
                return "image/png";
            }

            // JPEG signature
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF) {
                return "image/jpeg";
            }

            // WebP signature
            if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46) {
                return "image/webp";
            }

            return "image/jpeg"; // Default fallback
        }

        public async Task<Recipe> ImportStructuredAsync(ImportStructuredRequestDto dto) {
            using (logger.PushProperty("Source", dto.Source)) {
                logger.LogInformation("Starting structured import: {Title}", dto.Title);

                var recipe = new Recipe(
                    title: dto.Title,
                    yield: dto.Yield ?? 4,
                    prepTimeMinutes: dto.PrepTimeMinutes,
                    cookTimeMinutes: dto.CookTimeMinutes,
                    description: dto.Description,
                    source: dto.Source,
                    originalImageUrl: dto.OriginalImageUrl,
                    isPublic: false
                );

                // Parse raw ingredients
                if (dto.RawIngredients != null && dto.RawIngredients.Count > 0) {
                    var ingredients = new List<RecipeIngredient>();
                    for (int i = 0; i < dto.RawIngredients.Count; i++) {
                        ingredients.Add(new RecipeIngredient(
                            sortOrder: i + 1,
                            quantity: null,
                            unit: null,
                            item: null,
                            preparation: null,
                            rawText: dto.RawIngredients[i]
                        ));
                    }
                    recipe.SetIngredients(ingredients);
                }

                // Parse raw instructions
                if (dto.RawInstructions != null && dto.RawInstructions.Count > 0) {
                    var instructions = new List<RecipeInstruction>();
                    for (int i = 0; i < dto.RawInstructions.Count; i++) {
                        instructions.Add(new RecipeInstruction(
                            stepNumber: i + 1,
                            instruction: dto.RawInstructions[i],
                            rawText: dto.RawInstructions[i]
                        ));
                    }
                    recipe.SetInstructions(instructions);
                }

                // Save recipe
                await recipeRepository.AddAsync(recipe).ConfigureAwait(false);

                // Handle categories as tags
                if (dto.Categories != null && dto.Categories.Count > 0) {
                    await AssignCategoryTagsAsync(recipe, dto.Categories.ToArray()).ConfigureAwait(false);
                }

                logger.LogInformation("Successfully imported structured recipe: {Title}", recipe.Title);
                return recipe;
            }
        }

        public async Task<Recipe> ImportHtmlAsync(ImportHtmlRequestDto dto) {
            using (logger.PushProperty("Source", dto.Source)) {
                logger.LogInformation("Starting HTML import from: {Source}", dto.Source);

                if (string.IsNullOrWhiteSpace(dto.Html)) {
                    throw new InvalidOperationException("HTML content is required");
                }

                // Try schema.org extraction first
                var schemaRecipe = ExtractSchemaOrgRecipe(dto.Html);
                Recipe recipe;

                if (schemaRecipe != null) {
                    logger.LogInformation("Found schema.org/Recipe markup in HTML, parsing structured data");
                    recipe = MapSchemaOrgToRecipe(schemaRecipe, dto.Source);
                } else {
                    logger.LogInformation("No schema.org markup found in HTML, falling back to Gemini AI parsing");
                    recipe = await ParseWithGeminiAsync(dto.Html, dto.Source).ConfigureAwait(false);
                }

                // Save recipe
                await recipeRepository.AddAsync(recipe).ConfigureAwait(false);

                logger.LogInformation("Successfully imported HTML recipe: {Title}", recipe.Title);
                return recipe;
            }
        }

        private static string StripHtmlTags(string html) {
            if (string.IsNullOrWhiteSpace(html)) {
                return string.Empty;
            }

            // Replace block-level tags with newlines to preserve structure
            var text = Regex.Replace(html, @"</?(div|p|br|li|h[1-6]|tr|td)[^>]*>", "\n", RegexOptions.IgnoreCase);
            
            // Remove all other HTML tags
            text = Regex.Replace(text, @"<[^>]+>", string.Empty);
            
            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);
            
            // Clean up excessive whitespace
            text = Regex.Replace(text, @"\n\s*\n\s*\n", "\n\n"); // Max 2 newlines
            text = Regex.Replace(text, @"[ \t]+", " "); // Collapse spaces
            
            return text.Trim();
        }
    }
}
