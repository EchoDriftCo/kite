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

        private const string GroceryConsolidationPrompt = @"You are a grocery list optimizer. Given a list of grocery items (some with quantities and units, some without), consolidate them into a clean, deduplicated shopping list.

Return ONLY valid JSON matching this schema:
{
  ""items"": [
    {
      ""item"": ""string (normalized ingredient name)"",
      ""quantity"": ""number or null"",
      ""unit"": ""string or null (normalize to: cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)""
    }
  ]
}

Rules:
- Merge items that are clearly the same ingredient (e.g., ""cooking oil"" and ""oil"" → ""oil"", ""cheddar cheese"" and ""cheese"" → ""cheddar cheese"")
- When merging, keep the more specific name (""cheddar cheese"" over ""cheese"", ""olive oil"" over ""oil"")
- Combine quantities when units match or are compatible (e.g., 2 tsp + 1 tbsp = 1 tbsp + 2 tsp, or just sum as 5 tsp)
- If one entry has a quantity and another doesn't, include the quantity and add a note like ""+ more""
- If units are incompatible and can't be converted, list them separately (e.g., ""2 cups milk"" and ""1 lb milk"" stay separate)
- Normalize unit names (tablespoon → tbsp, teaspoon → tsp, etc.)
- Sort alphabetically by item name
- Do NOT merge items that are genuinely different ingredients";

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
- Always preserve raw text from image";

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

                    var parseResult = JsonSerializer.Deserialize<GeminiRecipeParseResult>(textPart,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

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
        /// Consolidate a grocery list using Gemini AI
        /// </summary>
        public async Task<GeminiGroceryConsolidationResponse> ConsolidateGroceryListAsync(List<GeminiGroceryItem> items, CancellationToken cancellationToken = default) {
            if (items == null || items.Count == 0) {
                return new GeminiGroceryConsolidationResponse();
            }

            // Build a text representation of the items
            var itemLines = items.Select(i => {
                var parts = new List<string>();
                if (i.Quantity.HasValue) {
                    parts.Add(i.Quantity.Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                }
                if (!string.IsNullOrWhiteSpace(i.Unit)) {
                    parts.Add(i.Unit);
                }
                parts.Add(i.Item);
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
                        Unit = i.Unit
                    }).ToList()
                };
            }

            logger.LogInformation("Successfully consolidated grocery list from {OriginalCount} to {ConsolidatedCount} items",
                items.Count, result.Items.Count);

            return new GeminiGroceryConsolidationResponse {
                Items = result.Items.Select(i => new GeminiConsolidatedItem {
                    Item = i.Item,
                    Quantity = i.Quantity,
                    Unit = i.Unit
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
    }
}
