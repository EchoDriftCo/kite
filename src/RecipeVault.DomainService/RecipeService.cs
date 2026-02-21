using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;
using RecipeVault.Integrations.Gemini;
using RecipeVault.Integrations.Gemini.Exceptions;

namespace RecipeVault.DomainService {
    public class RecipeService : IRecipeService {
        private readonly ILogger<RecipeService> logger;
        private readonly IRecipeRepository recipeRepository;
        private readonly ITagRepository tagRepository;
        private readonly IGeminiClient geminiClient;
        private readonly ISubjectPrincipal subjectPrincipal;
        private readonly IHttpClientFactory httpClientFactory;

        private static readonly Guid SystemSubjectId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public RecipeService(IRecipeRepository recipeRepository, ITagRepository tagRepository, IGeminiClient geminiClient, ILogger<RecipeService> logger, ISubjectPrincipal subjectPrincipal, IHttpClientFactory httpClientFactory) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
            this.tagRepository = tagRepository;
            this.geminiClient = geminiClient;
            this.subjectPrincipal = subjectPrincipal;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<Recipe> CreateRecipeAsync(UpdateRecipeDto dto) {
            var entity = new Recipe(dto.Title, dto.Yield, dto.PrepTimeMinutes, dto.CookTimeMinutes, dto.Description, dto.Source, dto.OriginalImageUrl, dto.IsPublic);

            if (!string.IsNullOrWhiteSpace(dto.SourceImageUrl)) {
                entity.SetSourceImageUrl(dto.SourceImageUrl);
            }

            if (dto.Ingredients != null) {
                entity.SetIngredients(dto.Ingredients.Select(i => new RecipeIngredient(i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText)).ToList());
            }

            if (dto.Instructions != null) {
                entity.SetInstructions(dto.Instructions.Select(i => new RecipeInstruction(i.StepNumber, i.Instruction, i.RawText)).ToList());
            }

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                await recipeRepository.AddAsync(entity);
                logger.LogInformation("Created new recipe");
                return entity;
            }
        }

        public async Task<Recipe> GetRecipeAsync(Guid recipeResourceId) {
            var entity = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (entity == null) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
            var isOwner = entity.CreatedSubject?.SubjectId == currentSubjectId;

            if (!isOwner && !entity.IsPublic) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }

            return entity;
        }

        private async Task<Recipe> GetOwnRecipeAsync(Guid recipeResourceId) {
            var entity = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (entity == null || entity.CreatedSubject?.SubjectId != Guid.Parse(subjectPrincipal.SubjectId)) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }
            return entity;
        }

        public Task<PagedList<Recipe>> SearchRecipesAsync(RecipeSearch search) {
            return recipeRepository.SearchAsync(search);
        }

        public async Task<Recipe> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto) {
            var entity = await GetOwnRecipeAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.Update(dto.Title, dto.Yield, dto.PrepTimeMinutes, dto.CookTimeMinutes, dto.Description, dto.Source, dto.OriginalImageUrl);
                entity.SetVisibility(dto.IsPublic);

                if (!string.IsNullOrWhiteSpace(dto.SourceImageUrl)) {
                    entity.SetSourceImageUrl(dto.SourceImageUrl);
                }

                if (dto.Ingredients != null) {
                    entity.SetIngredients(dto.Ingredients.Select(i => new RecipeIngredient(i.SortOrder, i.Quantity, i.Unit, i.Item, i.Preparation, i.RawText)).ToList());
                }

                if (dto.Instructions != null) {
                    entity.SetInstructions(dto.Instructions.Select(i => new RecipeInstruction(i.StepNumber, i.Instruction, i.RawText)).ToList());
                }

                logger.LogInformation("Updated existing recipe");
                return entity;
            }
        }

        public async Task SetRecipeVisibilityAsync(Guid recipeResourceId, bool isPublic) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.SetVisibility(isPublic);
                logger.LogInformation("Set recipe visibility to {IsPublic}", isPublic);
            }
        }

        public async Task DeleteRecipeAsync(Guid resourceId) {
            var entity = await GetOwnRecipeAsync(resourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                await recipeRepository.RemoveAsync(entity);
                logger.LogInformation("Deleted recipe");
            }
        }

        public async Task<Recipe> AssignTagsToRecipeAsync(Guid recipeResourceId, List<AssignTagDto> tags) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                foreach (var tagDto in tags) {
                    Tag tag;
                    bool isNewTag = false;

                    if (tagDto.TagResourceId.HasValue) {
                        tag = await tagRepository.GetAsync(tagDto.TagResourceId.Value).ConfigureAwait(false);
                        if (tag == null) {
                            throw new TagNotFoundException($"Tag with id {tagDto.TagResourceId} not found");
                        }
                    } else {
                        var category = (TagCategory)(tagDto.Category ?? (int)TagCategory.Custom);
                        tag = await tagRepository.GetByNameAndCategoryAsync(tagDto.Name, category).ConfigureAwait(false);
                        if (tag == null) {
                            tag = new Tag(tagDto.Name, category, isGlobal: false);
                            await tagRepository.AddAsync(tag);
                            isNewTag = true;
                        }
                    }

                    // Skip if already assigned and not overridden (only check for persisted tags)
                    if (!isNewTag) {
                        var existing = entity.RecipeTags.FirstOrDefault(rt => rt.TagId == tag.TagId);
                        if (existing != null) {
                            if (existing.IsOverridden) {
                                existing.ClearOverride();
                            }
                            continue;
                        }
                    }

                    // Use Tag entity overload for new tags (TagId not yet assigned),
                    // or TagId for persisted tags
                    if (isNewTag) {
                        entity.AddTag(new RecipeTag(entity.RecipeId, tag, currentSubjectId, isAiAssigned: false, confidence: null));
                    } else {
                        entity.AddTag(new RecipeTag(entity.RecipeId, tag.TagId, currentSubjectId, isAiAssigned: false, confidence: null));
                    }
                }

                logger.LogInformation("Assigned {TagCount} tags to recipe", tags.Count);
                return entity;
            }
        }

        public async Task<Recipe> RemoveTagFromRecipeAsync(Guid recipeResourceId, Guid tagResourceId) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            var tag = await tagRepository.GetAsync(tagResourceId).ConfigureAwait(false);
            if (tag == null) {
                throw new TagNotFoundException($"Tag with id {tagResourceId} not found");
            }

            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                var recipeTag = entity.RecipeTags.FirstOrDefault(rt => rt.TagId == tag.TagId);
                if (recipeTag != null) {
                    if (recipeTag.IsAiAssigned) {
                        // Mark overridden to prevent re-assignment on future AI analysis
                        recipeTag.MarkOverridden();
                    } else {
                        entity.RemoveTag(recipeTag);
                    }
                }

                logger.LogInformation("Removed tag {TagResourceId} from recipe", tagResourceId);
                return entity;
            }
        }

        public async Task AnalyzeAndApplyDietaryTagsAsync(Recipe recipe) {
            var ingredientTexts = recipe.Ingredients?.Select(i => {
                var parts = new List<string>();
                if (i.Quantity.HasValue) {
                    parts.Add(i.Quantity.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (!string.IsNullOrWhiteSpace(i.Unit)) {
                    parts.Add(i.Unit);
                }
                if (!string.IsNullOrWhiteSpace(i.Item)) {
                    parts.Add(i.Item);
                }
                return string.Join(" ", parts);
            }).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? new List<string>();

            if (ingredientTexts.Count == 0) {
                return;
            }

            try {
                var analysis = await geminiClient.AnalyzeDietaryTagsAsync(ingredientTexts).ConfigureAwait(false);

                foreach (var dietaryTag in analysis.Tags) {
                    var globalTag = await tagRepository.GetByNameAndCategoryAsync(dietaryTag.Name, TagCategory.Dietary).ConfigureAwait(false);
                    if (globalTag == null) {
                        logger.LogWarning("AI returned dietary tag '{TagName}' which does not exist in global tags, skipping", dietaryTag.Name);
                        continue;
                    }

                    var existing = recipe.RecipeTags.FirstOrDefault(rt => rt.TagId == globalTag.TagId);
                    if (existing != null) {
                        // Already assigned — skip even if IsOverridden, as the user explicitly removed it
                        continue;
                    }

                    recipe.AddTag(new RecipeTag(recipe.RecipeId, globalTag.TagId, SystemSubjectId, isAiAssigned: true, confidence: dietaryTag.Confidence));
                }

                logger.LogInformation("AI dietary analysis applied {TagCount} tags to recipe {RecipeResourceId}", analysis.Tags.Count, recipe.RecipeResourceId);
            }
            catch (Exception ex) {
                logger.LogWarning(ex, "AI dietary analysis failed for recipe {RecipeResourceId}, skipping", recipe.RecipeResourceId);
            }
        }

        public async Task SetRecipeRatingAsync(Guid recipeResourceId, int? rating) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.SetRating(rating);
                logger.LogInformation("Set recipe rating to {Rating}", rating);
            }
        }

        public async Task SetRecipeFavoriteAsync(Guid recipeResourceId, bool isFavorite) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.SetFavorite(isFavorite);
                logger.LogInformation("Set recipe favorite to {IsFavorite}", isFavorite);
            }
        }

        public async Task GenerateShareTokenAsync(Guid recipeResourceId) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.GenerateShareToken();
                logger.LogInformation("Generated share token for recipe");
            }
        }

        public async Task RevokeShareTokenAsync(Guid recipeResourceId) {
            var entity = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);
            using (logger.PushProperty("RecipeResourceId", entity.RecipeResourceId)) {
                entity.RevokeShareToken();
                logger.LogInformation("Revoked share token for recipe");
            }
        }

        public async Task<Recipe> GetRecipeByShareTokenAsync(string shareToken) {
            var entity = await recipeRepository.GetByShareTokenAsync(shareToken).ConfigureAwait(false);
            if (entity == null) {
                throw new RecipeNotFoundException("Shared recipe not found");
            }
            return entity;
        }

        public async Task<ParseRecipeResponseDto> ParseRecipeImageAsync(ParseRecipeRequestDto request) {
            GeminiParseResponse geminiResponse;
            string extractedImageUrl = null;

            if (!string.IsNullOrWhiteSpace(request.Url)) {
                logger.LogInformation("Parsing recipe from URL={Url}", request.Url);

                try {
                    var htmlContent = await FetchUrlContentAsync(request.Url).ConfigureAwait(false);
                    extractedImageUrl = ExtractOpenGraphImage(htmlContent);
                    var cleanedContent = StripHtmlNonContent(htmlContent);

                    geminiResponse = await geminiClient.ParseRecipeTextAsync(cleanedContent)
                        .ConfigureAwait(false);
                } catch (Exception ex) when (ex is not GeminiApiException) {
                    logger.LogError(ex, "Failed to parse recipe from URL {Url}", request.Url);
                    throw;
                }
            } else if (!string.IsNullOrWhiteSpace(request.Image)) {
                logger.LogInformation("Parsing recipe image, mimeType={MimeType}, imageSize={ImageSize}",
                    request.MimeType, request.Image?.Length ?? 0);

                try {
                    geminiResponse = await geminiClient.ParseRecipeAsync(request.Image, request.MimeType)
                        .ConfigureAwait(false);
                } catch (Exception ex) {
                    logger.LogError(ex, "Failed to parse recipe image");
                    throw;
                }
            } else {
                throw new ArgumentException("Either image data or URL is required");
            }

            var result = new ParseRecipeResponseDto {
                Confidence = geminiResponse.Confidence,
                Parsed = new ParsedRecipeDto {
                    Title = geminiResponse.Title,
                    Yield = geminiResponse.Yield,
                    PrepTimeMinutes = geminiResponse.PrepTimeMinutes,
                    CookTimeMinutes = geminiResponse.CookTimeMinutes,
                    Ingredients = geminiResponse.Ingredients?.Select(i => new ParsedIngredientDto {
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Item = i.Item,
                        Preparation = i.Preparation,
                        RawText = i.RawText
                    }).ToList(),
                    Instructions = geminiResponse.Instructions?.Select(i => new ParsedInstructionDto {
                        StepNumber = i.StepNumber,
                        Instruction = i.Instruction,
                        RawText = i.RawText
                    }).ToList(),
                    ImageUrl = extractedImageUrl
                },
                Warnings = geminiResponse.Warnings
            };

            logger.LogInformation("Successfully parsed recipe, confidence={Confidence}, warnings={WarningCount}",
                result.Confidence, result.Warnings?.Count ?? 0);

            return result;
        }

        private async Task<string> FetchUrlContentAsync(string url) {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https")) {
                throw new ArgumentException("Invalid URL. Please provide a valid HTTP or HTTPS URL.");
            }

            var client = httpClientFactory.CreateClient("RecipeUrlFetcher");

            var response = await client.GetAsync(uri).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private static string ExtractOpenGraphImage(string html) {
            // Try og:image meta tag first (most universal)
            var ogMatch = Regex.Match(html, @"<meta\s+[^>]*property\s*=\s*[""']og:image[""'][^>]*content\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            if (ogMatch.Success) {
                return ogMatch.Groups[1].Value;
            }

            // Try reverse attribute order (content before property)
            ogMatch = Regex.Match(html, @"<meta\s+[^>]*content\s*=\s*[""']([^""']+)[""'][^>]*property\s*=\s*[""']og:image[""']", RegexOptions.IgnoreCase);
            if (ogMatch.Success) {
                return ogMatch.Groups[1].Value;
            }

            return null;
        }

        private static string StripHtmlNonContent(string html) {
            // Remove script, style, nav, footer, header tags and their content
            var cleaned = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"<nav[^>]*>[\s\S]*?</nav>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"<footer[^>]*>[\s\S]*?</footer>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"<header[^>]*>[\s\S]*?</header>", "", RegexOptions.IgnoreCase);

            // Remove HTML comments
            cleaned = Regex.Replace(cleaned, @"<!--[\s\S]*?-->", "");

            // Remove remaining HTML tags but keep text content
            cleaned = Regex.Replace(cleaned, @"<[^>]+>", " ");

            // Decode common HTML entities
            cleaned = cleaned.Replace("&amp;", "&")
                             .Replace("&lt;", "<")
                             .Replace("&gt;", ">")
                             .Replace("&quot;", "\"")
                             .Replace("&#39;", "'")
                             .Replace("&nbsp;", " ");

            // Collapse whitespace
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            // Truncate to avoid exceeding Gemini token limits
            if (cleaned.Length > 30000) {
                cleaned = cleaned.Substring(0, 30000);
            }

            return cleaned;
        }
    }
}
