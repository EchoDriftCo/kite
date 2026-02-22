using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeVault.Integrations.Gemini.Exceptions;
using RecipeVault.Integrations.Gemini.Models;

namespace RecipeVault.Integrations.Gemini {
    /// <summary>
    /// Implementation of Gemini API client for recipe parsing
    /// </summary>
    public class GeminiClient : IGeminiClient {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly ILogger<GeminiClient> logger;

        private const string GroceryConsolidationPrompt = @"You are a grocery list optimizer. Given a list of grocery items (some with quantities and units, some without), consolidate them into a clean, deduplicated shopping list organized by store section.

Each item includes [from: ...] tags showing which recipes it comes from. Preserve and merge these source lists.

Return ONLY valid JSON matching this schema:
{
  ""items"": [
    {
      ""item"": ""string (normalized ingredient name)"",
      ""quantity"": ""number or null"",
      ""unit"": ""string or null (normalize to: cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)"",
      ""category"": ""string (one of: Produce, Dairy, Meat & Seafood, Bakery, Pantry, Frozen, Beverages, Condiments & Spices, Other)"",
      ""sources"": [""array of recipe name strings from the [from: ...] tags, deduplicated""]
    }
  ]
}

Rules:
- Merge items that are clearly the same ingredient (e.g., ""cooking oil"" and ""oil"" → ""oil"", ""cheddar cheese"" and ""cheese"" → ""cheddar cheese"")
- When merging, keep the more specific name (""cheddar cheese"" over ""cheese"", ""olive oil"" over ""oil"")
- When merging items, combine their sources arrays (deduplicated)
- Combine quantities when units match or are compatible (e.g., 2 tsp + 1 tbsp = 1 tbsp + 2 tsp, or just sum as 5 tsp)
- If one entry has a quantity and another doesn't, include the quantity and add a note like ""+ more""
- If units are incompatible and can't be converted, list them separately (e.g., ""2 cups milk"" and ""1 lb milk"" stay separate)
- Normalize unit names (tablespoon → tbsp, teaspoon → tsp, etc.)
- Assign each item to the most appropriate store category
- Sort items alphabetically within each category, and sort categories in this order: Produce, Dairy, Meat & Seafood, Bakery, Frozen, Beverages, Condiments & Spices, Pantry, Other
- Do NOT merge items that are genuinely different ingredients";

        private const string RecipeTextParserPrompt = @"You are a recipe parser. Extract structured data from this recipe webpage content.

Return ONLY valid JSON matching this schema:
{
  ""title"": ""string"",
  ""yield"": ""number or null (servings/portions)"",
  ""prepTimeMinutes"": ""number or null"",
  ""cookTimeMinutes"": ""number or null"",
  ""ingredients"": [
    {
      ""quantity"": ""number or null"",
      ""unit"": ""string or null (normalize to: cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)"",
      ""item"": ""string (the ingredient name)"",
      ""preparation"": ""string or null (e.g., chopped, melted, room temperature)"",
      ""rawText"": ""string (original text from source)""
    }
  ],
  ""instructions"": [
    {
      ""stepNumber"": ""number"",
      ""instruction"": ""string (cleaned up instruction)"",
      ""rawText"": ""string (original text from source)""
    }
  ]
}

Rules:
- Normalize units (tablespoon → tbsp, teaspoon → tsp, etc.)
- Convert fractions to decimals (1/2 → 0.5, 1/4 → 0.25, etc.)
- Separate preparation notes from ingredient names
- Number instructions sequentially even if source doesn't
- If information is unclear or missing, use null rather than guessing
- Always preserve original text from the source
- Ignore ads, navigation, comments, and other non-recipe content on the page

Time extraction:
- If the recipe explicitly states prep/cook times, use those values
- If times are NOT explicitly stated, calculate them from the instructions:
  - prepTimeMinutes = sum of active hands-on work (chopping, mixing, browning, assembling, etc.)
  - cookTimeMinutes = sum of ALL passive cooking times found in instructions (baking, simmering, boiling, roasting, resting, marinating, etc.)
  - Include every timed step — e.g. ""simmer 20 minutes"" + ""bake 1.5 hours"" = 110 cookTimeMinutes
- Convert all time units to minutes (1 hour = 60, 1.5 hours = 90, etc.)
- Do NOT double-count — if prep and cook overlap (e.g. ""while that bakes, prepare...""), keep them separate";

        private const string RecipeParserPrompt = @"You are a recipe parser. Extract structured data from this recipe image.

Return ONLY valid JSON matching this schema:
{
  ""title"": ""string"",
  ""yield"": ""number or null (servings/portions)"",
  ""prepTimeMinutes"": ""number or null"",
  ""cookTimeMinutes"": ""number or null"",
  ""ingredients"": [
    {
      ""quantity"": ""number or null"",
      ""unit"": ""string or null (normalize to: cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)"",
      ""item"": ""string (the ingredient name)"",
      ""preparation"": ""string or null (e.g., chopped, melted, room temperature)"",
      ""rawText"": ""string (exact text from image)""
    }
  ],
  ""instructions"": [
    {
      ""stepNumber"": ""number"",
      ""instruction"": ""string (cleaned up instruction)"",
      ""rawText"": ""string (exact text from image)""
    }
  ]
}

Rules:
- Normalize units (tablespoon → tbsp, teaspoon → tsp, etc.)
- Convert fractions to decimals (1/2 → 0.5, 1/4 → 0.25, etc.)
- Separate preparation notes from ingredient names
- Number instructions sequentially even if source doesn't
- If information is unclear or missing, use null rather than guessing
- Always preserve raw text from image

Time extraction:
- If the recipe explicitly states prep/cook times, use those values
- If times are NOT explicitly stated, calculate them from the instructions:
  - prepTimeMinutes = sum of active hands-on work (chopping, mixing, browning, assembling, etc.)
  - cookTimeMinutes = sum of ALL passive cooking times found in instructions (baking, simmering, boiling, roasting, resting, marinating, etc.)
  - Include every timed step — e.g. ""simmer 20 minutes"" + ""bake 1.5 hours"" = 110 cookTimeMinutes
- Convert all time units to minutes (1 hour = 60, 1.5 hours = 90, etc.)
- Do NOT double-count — if prep and cook overlap (e.g. ""while that bakes, prepare...""), keep them separate";

        /// <summary>
        /// Initializes a new instance of GeminiClient
        /// </summary>
        public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger) {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parse a recipe image using Gemini API
        /// </summary>
        public async Task<GeminiParseResponse> ParseRecipeAsync(string imageBase64, string mimeType, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(imageBase64)) {
                throw new ArgumentException("Image base64 data is required", nameof(imageBase64));
            }

            if (string.IsNullOrWhiteSpace(mimeType)) {
                throw new ArgumentException("MIME type is required", nameof(mimeType));
            }

            var apiKey = configuration["Gemini:ApiKey"] ??
                        Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey)) {
                throw new GeminiApiException("Gemini API key is not configured");
            }

            var model = configuration["Gemini:Model"] ??
                       Environment.GetEnvironmentVariable("GEMINI_MODEL") ??
                       "gemini-1.5-flash";

            var requestUri = $"/v1beta/models/{model}:generateContent?key={apiKey}";

            logger.LogInformation("Sending recipe parsing request to Gemini API, model={Model}, imageSize={ImageSize}",
                model, imageBase64.Length);

            try {
                var request = BuildGenerateContentRequest(imageBase64, mimeType);
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                using (logger.BeginScope(new Dictionary<string, object> {
                    { "GeminiModel", model },
                    { "ImageMimeType", mimeType }
                })) {
                    var response = await httpClient.PostAsync(requestUri, requestContent, cancellationToken)
                        .ConfigureAwait(false);

                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.BadRequest ||
                        response.StatusCode == HttpStatusCode.UnprocessableEntity) {
                        logger.LogWarning("Gemini API returned {StatusCode} - no recipe detected in image",
                            response.StatusCode);
                        throw new GeminiApiException(
                            "Could not identify a recipe in this image",
                            (int)response.StatusCode,
                            responseContent);
                    }

                    if (!response.IsSuccessStatusCode) {
                        logger.LogError("Gemini API error {StatusCode}: {ResponseContent}",
                            response.StatusCode, responseContent);
                        throw new GeminiApiException(
                            $"Gemini API request failed with status {response.StatusCode}",
                            (int)response.StatusCode,
                            responseContent);
                    }

                    logger.LogInformation("Received successful response from Gemini API: {ResponseContent}", responseContent);
                    var geminiResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    if (geminiResponse?.Error != null) {
                        logger.LogError("Gemini API returned error {ErrorCode}: {ErrorMessage}",
                            geminiResponse.Error.Code, geminiResponse.Error.Message);
                        throw new GeminiApiException(
                            $"Gemini API error: {geminiResponse.Error.Message}",
                            geminiResponse.Error.Code,
                            geminiResponse.Error.Message);
                    }

                    if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Count == 0) {
                        logger.LogWarning("Gemini API returned no candidates");
                        throw new GeminiApiException("Gemini API returned no response candidates");
                    }

                    var candidate = geminiResponse.Candidates[0];
                    if (candidate?.Content?.Parts == null || candidate.Content.Parts.Count == 0) {
                        logger.LogWarning("Gemini API returned no content parts");
                        throw new GeminiApiException("Gemini API returned no content");
                    }

                    var textPart = candidate.Content.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text))?.Text;
                    if (string.IsNullOrWhiteSpace(textPart)) {
                        logger.LogWarning("Gemini API returned no text content");
                        throw new GeminiApiException("Gemini API returned no text response");
                    }

                    // Strip markdown code fences if present
                    var jsonText = textPart.Trim();
                    if (jsonText.StartsWith("```", StringComparison.Ordinal))
                    {
                        var firstNewline = jsonText.IndexOf('\n');
                        if (firstNewline > 0)
                            jsonText = jsonText.Substring(firstNewline + 1);
                        if (jsonText.EndsWith("```", StringComparison.Ordinal))
                            jsonText = jsonText.Substring(0, jsonText.Length - 3).Trim();
                    }

                    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    GeminiRecipeParseResult parseResult;

                    // Handle Gemini returning an array of recipes (multi-recipe photos)
                    if (jsonText.TrimStart().StartsWith('['))
                    {
                        var results = JsonSerializer.Deserialize<List<GeminiRecipeParseResult>>(jsonText, jsonOptions);
                        if (results == null || results.Count == 0)
                        {
                            logger.LogWarning("Gemini returned an empty array of recipes");
                            throw new GeminiApiException("Could not parse Gemini response as recipe data");
                        }

                        logger.LogInformation("Gemini returned {Count} recipes, using first one: {Title}",
                            results.Count, results[0].Title);
                        parseResult = results[0];
                    }
                    else
                    {
                        parseResult = JsonSerializer.Deserialize<GeminiRecipeParseResult>(jsonText, jsonOptions);
                    }

                    if (parseResult == null) {
                        logger.LogWarning("Failed to deserialize recipe parse result");
                        throw new GeminiApiException("Could not parse Gemini response as recipe data");
                    }

                    var response_model = MapToGeminiParseResponse(parseResult);
                    logger.LogInformation("Successfully parsed recipe from image, confidence={Confidence}",
                        response_model.Confidence);

                    return response_model;
                }
            }
            catch (HttpRequestException ex) {
                logger.LogError(ex, "HTTP request to Gemini API failed");
                throw new GeminiApiException("Failed to communicate with Gemini API", ex);
            }
            catch (TaskCanceledException ex) {
                logger.LogError(ex, "Gemini API request timed out");
                throw new GeminiApiException("Gemini API request timed out", ex);
            }
            catch (GeminiApiException) {
                throw;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Unexpected error while parsing recipe with Gemini");
                throw new GeminiApiException("Unexpected error while parsing recipe", ex);
            }
        }

        /// <summary>
        /// Parse recipe text (e.g., from a webpage) using Gemini API
        /// </summary>
        public async Task<GeminiParseResponse> ParseRecipeTextAsync(string recipeText, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(recipeText)) {
                throw new ArgumentException("Recipe text is required", nameof(recipeText));
            }

            logger.LogInformation("Sending recipe text parsing request to Gemini API, textLength={TextLength}", recipeText.Length);

            var prompt = RecipeTextParserPrompt + "\n\nRecipe content:\n" + recipeText;
            var responseText = await SendTextPromptAsync(prompt, cancellationToken).ConfigureAwait(false);

            // Strip markdown code fences if present
            var jsonText = responseText.Trim();
            if (jsonText.StartsWith("```", StringComparison.Ordinal)) {
                var firstNewline = jsonText.IndexOf('\n');
                if (firstNewline > 0)
                    jsonText = jsonText.Substring(firstNewline + 1);
                if (jsonText.EndsWith("```", StringComparison.Ordinal))
                    jsonText = jsonText.Substring(0, jsonText.Length - 3).Trim();
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            GeminiRecipeParseResult parseResult;

            if (jsonText.TrimStart().StartsWith('[')) {
                var results = JsonSerializer.Deserialize<List<GeminiRecipeParseResult>>(jsonText, jsonOptions);
                if (results == null || results.Count == 0) {
                    logger.LogWarning("Gemini returned an empty array of recipes from text");
                    throw new GeminiApiException("Could not parse Gemini response as recipe data");
                }
                logger.LogInformation("Gemini returned {Count} recipes from text, using first one: {Title}", results.Count, results[0].Title);
                parseResult = results[0];
            } else {
                parseResult = JsonSerializer.Deserialize<GeminiRecipeParseResult>(jsonText, jsonOptions);
            }

            if (parseResult == null) {
                logger.LogWarning("Failed to deserialize recipe parse result from text");
                throw new GeminiApiException("Could not parse Gemini response as recipe data");
            }

            var response = MapToGeminiParseResponse(parseResult);
            logger.LogInformation("Successfully parsed recipe from text, confidence={Confidence}", response.Confidence);
            return response;
        }

        /// <summary>
        /// Consolidate a grocery list using Gemini AI
        /// </summary>
        public async Task<GeminiGroceryConsolidationResponse> ConsolidateGroceryListAsync(List<GeminiGroceryItem> items, CancellationToken cancellationToken = default) {
            if (items == null || items.Count == 0) {
                return new GeminiGroceryConsolidationResponse();
            }

            // Build a text representation of the items including sources
            var itemLines = items.Select(i => {
                var parts = new List<string>();
                if (i.Quantity.HasValue) {
                    parts.Add(i.Quantity.Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                }
                if (!string.IsNullOrWhiteSpace(i.Unit)) {
                    parts.Add(i.Unit);
                }
                parts.Add(i.Item);
                if (i.Sources != null && i.Sources.Count > 0) {
                    parts.Add($"[from: {string.Join(", ", i.Sources)}]");
                }
                return string.Join(" ", parts);
            });

            var prompt = GroceryConsolidationPrompt + "\n\nHere are the grocery items to consolidate:\n" +
                         string.Join("\n", itemLines.Select(l => $"- {l}"));

            logger.LogInformation("Sending grocery consolidation request to Gemini API, itemCount={ItemCount}", items.Count);

            var textPart = await SendTextPromptAsync(prompt, cancellationToken).ConfigureAwait(false);

            var result = JsonSerializer.Deserialize<GeminiGroceryConsolidationResult>(textPart,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result?.Items == null) {
                logger.LogWarning("Failed to deserialize grocery consolidation result, returning original items");
                return new GeminiGroceryConsolidationResponse {
                    Items = items.Select(i => new GeminiConsolidatedItem {
                        Item = i.Item,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Category = "Other",
                        Sources = i.Sources ?? new List<string>()
                    }).ToList()
                };
            }

            logger.LogInformation("Successfully consolidated grocery list from {OriginalCount} to {ConsolidatedCount} items",
                items.Count, result.Items.Count);

            return new GeminiGroceryConsolidationResponse {
                Items = result.Items.Select(i => new GeminiConsolidatedItem {
                    Item = i.Item,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Category = i.Category ?? "Other",
                    Sources = i.Sources ?? new List<string>()
                }).ToList()
            };
        }

        private const string DietaryAnalysisPrompt = @"You are a dietary analysis expert. Given a list of recipe ingredients, determine which dietary categories this recipe qualifies for.

Return ONLY valid JSON matching this schema:
{
  ""tags"": [
    {
      ""name"": ""string (exactly one of: Vegan, Vegetarian, Gluten-Free, Dairy-Free, Nut-Free, Keto, Paleo, Low-Carb, Sugar-Free, Whole30, Halal, Kosher)"",
      ""confidence"": number (0.0-1.0)
    }
  ]
}

Rules:
- Only include tags where confidence >= 0.6
- Vegan: no animal products whatsoever (no meat, dairy, eggs, honey)
- Vegetarian: no meat or fish, but dairy/eggs are allowed
- Gluten-Free: no wheat, barley, rye, or ingredients that typically contain gluten
- Dairy-Free: no milk, cheese, butter, cream, yogurt, whey
- Nut-Free: no tree nuts or peanuts
- Keto: very low carb (no grains, minimal sugar, starchy vegetables)
- Paleo: no grains, legumes, dairy, refined sugar, or processed foods
- Low-Carb: limited grains, sugar, and starchy vegetables
- Sugar-Free: no added sugars or sweet ingredients
- Whole30: no grains, dairy, legumes, sugar, alcohol, or soy
- Halal: no pork or alcohol; meat must be halal-slaughtered
- Kosher: no pork, shellfish; no mixing meat and dairy
- Be conservative — if uncertain, do NOT include the tag
- An empty tags array is a valid response if no categories apply";

        /// <summary>
        /// Analyze recipe ingredients to infer dietary tags
        /// </summary>
        public async Task<GeminiDietaryAnalysisResponse> AnalyzeDietaryTagsAsync(List<string> ingredients, CancellationToken cancellationToken = default) {
            if (ingredients == null || ingredients.Count == 0) {
                return new GeminiDietaryAnalysisResponse();
            }

            var prompt = DietaryAnalysisPrompt + "\n\nHere are the recipe ingredients to analyze:\n" +
                         string.Join("\n", ingredients.Select(i => $"- {i}"));

            logger.LogInformation("Sending dietary analysis request to Gemini API, ingredientCount={IngredientCount}", ingredients.Count);

            var textPart = await SendTextPromptAsync(prompt, cancellationToken).ConfigureAwait(false);

            var result = JsonSerializer.Deserialize<GeminiDietaryAnalysisResult>(textPart,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result?.Tags == null) {
                logger.LogWarning("Failed to deserialize dietary analysis result, returning empty tags");
                return new GeminiDietaryAnalysisResponse();
            }

            logger.LogInformation("Dietary analysis found {TagCount} tags", result.Tags.Count);

            return new GeminiDietaryAnalysisResponse {
                Tags = result.Tags.Select(t => new GeminiDietaryTag {
                    Name = t.Name,
                    Confidence = t.Confidence
                }).ToList()
            };
        }

        private const string EntityNormalizationPrompt = @"You are an entity recognition expert. Determine if the given name is a known chef/restaurant/cookbook.

Return ONLY valid JSON matching this schema:
{{
  ""isRecognized"": boolean,
  ""canonicalName"": ""string or null (official name if recognized)"",
  ""normalizedEntityId"": ""string or null (lowercase slug form, e.g., 'bobby-flay')"",
  ""confidence"": number (0.0-1.0)
}}

Rules:
- isRecognized should only be true if you are confident this is a well-known entity
- For Chefs: must be a professional, famous, or well-known chef (e.g., ""Bobby Flay"", ""Gordon Ramsay"")
- For Restaurants: must be a recognizable restaurant chain or famous establishment (e.g., ""Olive Garden"", ""The French Laundry"")
- For Cookbooks: must be a published cookbook with recognizable title and author
- canonicalName should be the official/most common name (proper capitalization)
- normalizedEntityId should be lowercase with hyphens (e.g., ""bobby-flay"", ""the-french-laundry"")
- confidence should reflect your certainty (1.0 = very confident, 0.5 = somewhat sure, 0.0 = not recognized)
- If not recognized or confidence < 0.6, set isRecognized to false";

        /// <summary>
        /// Normalize an entity name to check if it's a known entity
        /// </summary>
        public async Task<GeminiEntityNormalizationResponse> NormalizeEntityAsync(string entityName, int sourceType, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                return new GeminiEntityNormalizationResponse { IsRecognized = false };
            }

            // Only normalize Chef, Restaurant, or Cookbook (values 2, 3, 4)
            if (sourceType < 2 || sourceType > 4) {
                return new GeminiEntityNormalizationResponse { IsRecognized = false };
            }

            var entityTypeName = sourceType switch {
                2 => "chef",
                3 => "restaurant",
                4 => "cookbook",
                _ => null
            };

            if (entityTypeName == null) {
                return new GeminiEntityNormalizationResponse { IsRecognized = false };
            }

            var prompt = EntityNormalizationPrompt + $"\n\nEntity type: {entityTypeName}\nEntity name to check: {entityName}";

            logger.LogInformation("Sending entity normalization request to Gemini API, entityName={EntityName}, type={Type}", entityName, entityTypeName);

            try {
                var textPart = await SendTextPromptAsync(prompt, cancellationToken).ConfigureAwait(false);

                // Strip markdown code fences if present
                var jsonText = textPart.Trim();
                if (jsonText.StartsWith("```", StringComparison.Ordinal)) {
                    var firstNewline = jsonText.IndexOf('\n');
                    if (firstNewline > 0)
                        jsonText = jsonText.Substring(firstNewline + 1);
                    if (jsonText.EndsWith("```", StringComparison.Ordinal))
                        jsonText = jsonText.Substring(0, jsonText.Length - 3).Trim();
                }

                var result = JsonSerializer.Deserialize<GeminiEntityNormalizationResponse>(jsonText,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (result == null) {
                    logger.LogWarning("Failed to deserialize entity normalization result for {EntityName}", entityName);
                    return new GeminiEntityNormalizationResponse { IsRecognized = false };
                }

                logger.LogInformation("Entity normalization for {EntityName}: isRecognized={IsRecognized}, canonicalName={CanonicalName}, confidence={Confidence}",
                    entityName, result.IsRecognized, result.CanonicalName, result.Confidence);

                return result;
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "Error normalizing entity {EntityName}, returning not recognized", entityName);
                return new GeminiEntityNormalizationResponse { IsRecognized = false };
            }
        }

        /// <summary>
        /// Send a text-only prompt to Gemini and return the text response
        /// </summary>
        private async Task<string> SendTextPromptAsync(string prompt, CancellationToken cancellationToken) {
            var apiKey = configuration["Gemini:ApiKey"] ??
                        Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey)) {
                throw new GeminiApiException("Gemini API key is not configured");
            }

            var model = configuration["Gemini:Model"] ??
                       Environment.GetEnvironmentVariable("GEMINI_MODEL") ??
                       "gemini-1.5-flash";

            var requestUri = $"/v1beta/models/{model}:generateContent?key={apiKey}";

            try {
                var request = BuildTextOnlyRequest(prompt);
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(requestUri, requestContent, cancellationToken)
                    .ConfigureAwait(false);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {
                    logger.LogError("Gemini API error {StatusCode}: {ResponseContent}",
                        response.StatusCode, responseContent);
                    throw new GeminiApiException(
                        $"Gemini API request failed with status {response.StatusCode}",
                        (int)response.StatusCode,
                        responseContent);
                }

                var geminiResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (geminiResponse?.Error != null) {
                    throw new GeminiApiException(
                        $"Gemini API error: {geminiResponse.Error.Message}",
                        geminiResponse.Error.Code,
                        geminiResponse.Error.Message);
                }

                if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Count == 0) {
                    throw new GeminiApiException("Gemini API returned no response candidates");
                }

                var candidate = geminiResponse.Candidates[0];
                if (candidate?.Content?.Parts == null || candidate.Content.Parts.Count == 0) {
                    throw new GeminiApiException("Gemini API returned no content");
                }

                var textPart = candidate.Content.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text))?.Text;
                if (string.IsNullOrWhiteSpace(textPart)) {
                    throw new GeminiApiException("Gemini API returned no text response");
                }

                return textPart;
            }
            catch (HttpRequestException ex) {
                logger.LogError(ex, "HTTP request to Gemini API failed");
                throw new GeminiApiException("Failed to communicate with Gemini API", ex);
            }
            catch (TaskCanceledException ex) {
                logger.LogError(ex, "Gemini API request timed out");
                throw new GeminiApiException("Gemini API request timed out", ex);
            }
            catch (GeminiApiException) {
                throw;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Unexpected error communicating with Gemini API");
                throw new GeminiApiException("Unexpected error communicating with Gemini API", ex);
            }
        }

        /// <summary>
        /// Build a text-only request (no image) for Gemini generateContent API
        /// </summary>
        private GeminiGenerateContentRequest BuildTextOnlyRequest(string prompt) {
            return new GeminiGenerateContentRequest {
                Contents = new List<GeminiContent> {
                    new GeminiContent {
                        Parts = new List<GeminiPart> {
                            new GeminiPart {
                                Text = prompt
                            }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig {
                    ResponseMimeType = "application/json"
                }
            };
        }

        /// <summary>
        /// Build the request object for Gemini generateContent API
        /// </summary>
        private GeminiGenerateContentRequest BuildGenerateContentRequest(string imageBase64, string mimeType) {
            return new GeminiGenerateContentRequest {
                Contents = new List<GeminiContent> {
                    new GeminiContent {
                        Parts = new List<GeminiPart> {
                            new GeminiPart {
                                Text = RecipeParserPrompt
                            },
                            new GeminiPart {
                                InlineData = new GeminiInlineData {
                                    MimeType = mimeType,
                                    Data = imageBase64
                                }
                            }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig {
                    ResponseMimeType = "application/json"
                }
            };
        }

        /// <summary>
        /// Map Gemini parse result to domain response model
        /// </summary>
        private GeminiParseResponse MapToGeminiParseResponse(GeminiRecipeParseResult parseResult) {
            var warnings = new List<string>();

            // Calculate confidence score based on field presence and completeness
            var confidence = CalculateConfidence(parseResult, warnings);

            return new GeminiParseResponse {
                Confidence = confidence,
                Title = parseResult.Title,
                Yield = parseResult.Yield,
                PrepTimeMinutes = parseResult.PrepTimeMinutes,
                CookTimeMinutes = parseResult.CookTimeMinutes,
                Ingredients = parseResult.Ingredients?.Select(i => new GeminiIngredient {
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Item = i.Item,
                    Preparation = i.Preparation,
                    RawText = i.RawText
                }).ToList() ?? new List<GeminiIngredient>(),
                Instructions = parseResult.Instructions?.Select(i => new GeminiInstruction {
                    StepNumber = i.StepNumber,
                    Instruction = i.Instruction,
                    RawText = i.RawText
                }).ToList() ?? new List<GeminiInstruction>(),
                Warnings = warnings
            };
        }

        /// <summary>
        /// Calculate confidence score (0-1) based on parse completeness
        /// </summary>
        private double CalculateConfidence(GeminiRecipeParseResult result, List<string> warnings) {
            if (result == null) {
                return 0;
            }

            double score = 0;
            double maxScore = 0;

            // Title (0.3 points)
            maxScore += 0.3;
            if (!string.IsNullOrWhiteSpace(result.Title)) {
                score += 0.3;
            } else {
                warnings.Add("Recipe title could not be determined");
            }

            // Yield (0.15 points)
            maxScore += 0.15;
            if (result.Yield.HasValue && result.Yield > 0) {
                score += 0.15;
            } else {
                warnings.Add("Recipe yield (servings) could not be determined");
            }

            // Times (0.2 points)
            maxScore += 0.2;
            if (result.PrepTimeMinutes.HasValue && result.PrepTimeMinutes > 0) {
                score += 0.1;
            } else {
                warnings.Add("Prep time could not be determined");
            }

            if (result.CookTimeMinutes.HasValue && result.CookTimeMinutes > 0) {
                score += 0.1;
            } else {
                warnings.Add("Cook time could not be determined");
            }

            // Ingredients (0.2 points)
            maxScore += 0.2;
            if (result.Ingredients?.Count > 0) {
                var validIngredients = result.Ingredients.Count(i =>
                    !string.IsNullOrWhiteSpace(i.Item) &&
                    (!string.IsNullOrWhiteSpace(i.Unit) || i.Quantity.HasValue || !string.IsNullOrWhiteSpace(i.RawText)));

                if (validIngredients > 0) {
                    score += 0.2 * (Math.Min(validIngredients, 3) / 3.0); // Up to 0.2 for ingredients
                }
            } else {
                warnings.Add("No ingredients were detected");
            }

            // Instructions (0.15 points)
            maxScore += 0.15;
            if (result.Instructions?.Count > 0) {
                var validInstructions = result.Instructions.Count(i =>
                    !string.IsNullOrWhiteSpace(i.Instruction) || !string.IsNullOrWhiteSpace(i.RawText));

                if (validInstructions > 0) {
                    score += 0.15 * (Math.Min(validInstructions, 3) / 3.0); // Up to 0.15 for instructions
                }
            } else {
                warnings.Add("No instructions were detected");
            }

            return maxScore > 0 ? score / maxScore : 0;
        }

        private const string SubstitutionPromptSpecific = @"You are a culinary expert helping with ingredient substitutions.

Recipe: {recipeTitle}

Full ingredient list (for context):
{allIngredients}

Instructions summary:
{instructionsSummary}

Please suggest substitutions for these specific ingredients:
{targetIngredients}

Dietary constraints to adhere to:
{dietaryConstraints}

For each ingredient, provide 2-3 substitution options. Each option should include:
1. Replacement ingredient(s) with exact quantities
2. Brief note on taste/texture impact
3. Any technique adjustments needed

Return ONLY valid JSON matching this schema:
{{
  ""analysis"": ""string (optional brief overview)"",
  ""substitutions"": [
    {{
      ""originalIndex"": number,
      ""original"": ""string (original ingredient text)"",
      ""reason"": ""string or null (why this needs substitution)"",
      ""options"": [
        {{
          ""name"": ""string (descriptive name for this option)"",
          ""ingredients"": [
            {{
              ""quantity"": number or null,
              ""unit"": ""string or null"",
              ""item"": ""string""
            }}
          ],
          ""notes"": ""string (taste/texture impact)"",
          ""techniqueAdjustments"": ""string or null""
        }}
      ]
    }}
  ]
}}";

        private const string SubstitutionPromptDietary = @"You are a culinary expert helping adapt recipes for dietary needs.

Recipe: {recipeTitle}

Full ingredient list:
{allIngredients}

Instructions:
{instructionsSummary}

Evaluate this recipe for the following dietary constraints:
{dietaryConstraints}

Identify ALL ingredients that need substitution to meet these constraints.
For each problematic ingredient, provide 2-3 substitution options.

Return ONLY valid JSON matching this schema:
{{
  ""analysis"": ""string (brief overview of what needs changing)"",
  ""substitutions"": [
    {{
      ""originalIndex"": number,
      ""original"": ""string (original ingredient text)"",
      ""reason"": ""string (why this violates the constraint)"",
      ""options"": [
        {{
          ""name"": ""string (descriptive name for this option)"",
          ""ingredients"": [
            {{
              ""quantity"": number or null,
              ""unit"": ""string or null"",
              ""item"": ""string""
            }}
          ],
          ""notes"": ""string (taste/texture impact)"",
          ""techniqueAdjustments"": ""string or null""
        }}
      ]
    }}
  ]
}}";

        /// <summary>
        /// Analyze recipe and suggest ingredient substitutions
        /// </summary>
        public async Task<GeminiSubstitutionAnalysis> AnalyzeSubstitutionsAsync(
            string recipeTitle,
            List<string> allIngredients,
            string instructionsSummary,
            List<int> targetIngredientIndices,
            List<string> dietaryConstraints,
            CancellationToken cancellationToken = default) {

            if (string.IsNullOrWhiteSpace(recipeTitle)) {
                throw new ArgumentException("Recipe title is required", nameof(recipeTitle));
            }

            if (allIngredients == null || allIngredients.Count == 0) {
                throw new ArgumentException("Ingredients list is required", nameof(allIngredients));
            }

            var hasTargetIngredients = targetIngredientIndices != null && targetIngredientIndices.Count > 0;
            var hasConstraints = dietaryConstraints != null && dietaryConstraints.Count > 0;

            if (!hasTargetIngredients && !hasConstraints) {
                throw new ArgumentException("Must specify either target ingredients or dietary constraints");
            }

            // Build the prompt based on mode
            string prompt;
            if (hasTargetIngredients) {
                // Mode A: Specific ingredients
                var targetIngredientsText = string.Join("\n", 
                    targetIngredientIndices.Select(i => $"- {allIngredients[i]}"));
                
                prompt = SubstitutionPromptSpecific
                    .Replace("{recipeTitle}", recipeTitle)
                    .Replace("{allIngredients}", string.Join("\n", allIngredients.Select((ing, i) => $"{i}. {ing}")))
                    .Replace("{instructionsSummary}", instructionsSummary ?? "N/A")
                    .Replace("{targetIngredients}", targetIngredientsText)
                    .Replace("{dietaryConstraints}", hasConstraints ? string.Join(", ", dietaryConstraints) : "None");
            } else {
                // Mode B: Dietary constraints only
                prompt = SubstitutionPromptDietary
                    .Replace("{recipeTitle}", recipeTitle)
                    .Replace("{allIngredients}", string.Join("\n", allIngredients.Select((ing, i) => $"{i}. {ing}")))
                    .Replace("{instructionsSummary}", instructionsSummary ?? "N/A")
                    .Replace("{dietaryConstraints}", string.Join(", ", dietaryConstraints));
            }

            logger.LogInformation("Sending substitution analysis request to Gemini API, recipeTitle={RecipeTitle}, targetCount={TargetCount}, constraints={Constraints}",
                recipeTitle, targetIngredientIndices?.Count ?? 0, string.Join(",", dietaryConstraints ?? new List<string>()));

            var textPart = await SendTextPromptAsync(prompt, cancellationToken).ConfigureAwait(false);

            // Strip markdown code fences if present
            var jsonText = textPart.Trim();
            if (jsonText.StartsWith("```", StringComparison.Ordinal)) {
                var firstNewline = jsonText.IndexOf('\n');
                if (firstNewline > 0)
                    jsonText = jsonText.Substring(firstNewline + 1);
                if (jsonText.EndsWith("```", StringComparison.Ordinal))
                    jsonText = jsonText.Substring(0, jsonText.Length - 3).Trim();
            }

            var result = JsonSerializer.Deserialize<GeminiSubstitutionAnalysis>(jsonText,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result == null) {
                logger.LogWarning("Failed to deserialize substitution analysis result");
                throw new GeminiApiException("Could not parse Gemini response as substitution data");
            }

            logger.LogInformation("Successfully analyzed substitutions, found {Count} ingredients with substitution options",
                result.Substitutions?.Count ?? 0);

            return result;
        }
    }
}
