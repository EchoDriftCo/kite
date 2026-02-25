using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cortside.Common.Logging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService.Models;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class ExportService : IExportService {
        private readonly ILogger<ExportService> logger;
        private readonly IRecipeRepository recipeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;
        
        private static readonly JsonSerializerOptions JsonOptions = new() {
            WriteIndented = true
        };

        public ExportService(
            IRecipeRepository recipeRepository,
            ISubjectPrincipal subjectPrincipal,
            ILogger<ExportService> logger) {
            this.logger = logger;
            this.recipeRepository = recipeRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        public async Task<string> ExportRecipeAsJsonAsync(Guid recipeResourceId) {
            var recipe = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", recipe.RecipeResourceId)) {
                var exportData = new {
                    recipe.RecipeResourceId,
                    recipe.Title,
                    recipe.Description,
                    recipe.Yield,
                    recipe.PrepTimeMinutes,
                    recipe.CookTimeMinutes,
                    recipe.TotalTimeMinutes,
                    recipe.Source,
                    recipe.OriginalImageUrl,
                    recipe.SourceImageUrl,
                    recipe.Rating,
                    recipe.IsFavorite,
                    Ingredients = recipe.Ingredients?.Select(i => new {
                        i.SortOrder,
                        i.Quantity,
                        i.Unit,
                        i.Item,
                        i.Preparation,
                        i.RawText
                    }).ToList(),
                    Instructions = recipe.Instructions?.Select(i => new {
                        i.StepNumber,
                        i.Instruction,
                        i.RawText
                    }).ToList(),
                    Tags = recipe.RecipeTags?.Select(rt => new {
                        rt.Tag.Name,
                        rt.Tag.Category,
                        rt.Detail
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(exportData, JsonOptions);

                logger.LogInformation("Exported recipe as JSON");
                return json;
            }
        }

        public async Task<string> ExportRecipeAsTextAsync(Guid recipeResourceId) {
            var recipe = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", recipe.RecipeResourceId)) {
                var sb = new StringBuilder();
                
                // Title
                sb.AppendLine(recipe.Title.ToUpperInvariant());
                sb.AppendLine(new string('=', recipe.Title.Length));
                sb.AppendLine();

                // Metadata
                if (!string.IsNullOrWhiteSpace(recipe.Source)) {
                    sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Source: {recipe.Source}");
                }
                sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Servings: {recipe.Yield}");
                if (recipe.PrepTimeMinutes.HasValue) {
                    sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Prep Time: {recipe.PrepTimeMinutes} minutes");
                }
                if (recipe.CookTimeMinutes.HasValue) {
                    sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Cook Time: {recipe.CookTimeMinutes} minutes");
                }
                if (recipe.TotalTimeMinutes.HasValue) {
                    sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Total Time: {recipe.TotalTimeMinutes} minutes");
                }
                sb.AppendLine();

                // Description
                if (!string.IsNullOrWhiteSpace(recipe.Description)) {
                    sb.AppendLine(recipe.Description);
                    sb.AppendLine();
                }

                // Ingredients
                sb.AppendLine("INGREDIENTS");
                sb.AppendLine("------------");
                if (recipe.Ingredients != null && recipe.Ingredients.Count > 0) {
                    foreach (var ingredient in recipe.Ingredients.OrderBy(i => i.SortOrder)) {
                        if (!string.IsNullOrWhiteSpace(ingredient.RawText)) {
                            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"• {ingredient.RawText}");
                        } else {
                            var parts = new List<string>();
                            if (ingredient.Quantity.HasValue) {
                                parts.Add(ingredient.Quantity.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            }
                            if (!string.IsNullOrWhiteSpace(ingredient.Unit)) {
                                parts.Add(ingredient.Unit);
                            }
                            if (!string.IsNullOrWhiteSpace(ingredient.Item)) {
                                parts.Add(ingredient.Item);
                            }
                            if (!string.IsNullOrWhiteSpace(ingredient.Preparation)) {
                                parts.Add($"({ingredient.Preparation})");
                            }
                            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"• {string.Join(" ", parts)}");
                        }
                    }
                } else {
                    sb.AppendLine("(None)");
                }
                sb.AppendLine();

                // Instructions
                sb.AppendLine("INSTRUCTIONS");
                sb.AppendLine("-------------");
                if (recipe.Instructions != null && recipe.Instructions.Count > 0) {
                    foreach (var instruction in recipe.Instructions.OrderBy(i => i.StepNumber)) {
                        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"{instruction.StepNumber}. {instruction.Instruction}");
                        sb.AppendLine();
                    }
                } else {
                    sb.AppendLine("(None)");
                    sb.AppendLine();
                }

                // Tags
                if (recipe.RecipeTags != null && recipe.RecipeTags.Count > 0) {
                    sb.AppendLine("TAGS");
                    sb.AppendLine("----");
                    foreach (var recipeTag in recipe.RecipeTags) {
                        var tagText = recipeTag.Tag.Name;
                        if (!string.IsNullOrWhiteSpace(recipeTag.Detail)) {
                            tagText += $": {recipeTag.Detail}";
                        }
                        sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"• {tagText}");
                    }
                    sb.AppendLine();
                }

                logger.LogInformation("Exported recipe as text");
                return sb.ToString();
            }
        }

        public async Task<byte[]> ExportRecipeAsPaprikaAsync(Guid recipeResourceId) {
            var recipe = await GetOwnRecipeAsync(recipeResourceId).ConfigureAwait(false);

            using (logger.PushProperty("RecipeResourceId", recipe.RecipeResourceId)) {
                var paprikaRecipe = MapToPaprikaRecipe(recipe);

                // Serialize to JSON
                var json = JsonSerializer.Serialize(new[] { paprikaRecipe });

                // Compress with gzip
                var gzippedData = CompressToGzip(json);

                logger.LogInformation("Exported recipe as Paprika format");
                return gzippedData;
            }
        }

        public async Task<byte[]> ExportAllAsPaprikaAsync(Guid subjectId) {
            using (logger.PushProperty("SubjectId", subjectId)) {
                // Get all recipes for this user
                var search = new RecipeSearch {
                    CreatedSubjectId = subjectId,
                    PageNumber = 1,
                    PageSize = int.MaxValue // Get all recipes
                };

                var recipes = await recipeRepository.SearchAsync(search).ConfigureAwait(false);

                if (recipes.Items.Count == 0) {
                    logger.LogWarning("No recipes found for export");
                    // Return empty gzipped array
                    return CompressToGzip("[]");
                }

                logger.LogInformation("Exporting {RecipeCount} recipes", recipes.Items.Count);

                // Convert all recipes to Paprika format
                var paprikaRecipes = recipes.Items.Select(MapToPaprikaRecipe).ToList();

                // Serialize to JSON
                var json = JsonSerializer.Serialize(paprikaRecipes);

                // Compress with gzip
                var gzippedData = CompressToGzip(json);

                logger.LogInformation("Exported all recipes as Paprika format");
                return gzippedData;
            }
        }

        private async Task<Recipe> GetOwnRecipeAsync(Guid recipeResourceId) {
            var entity = await recipeRepository.GetAsync(recipeResourceId).ConfigureAwait(false);
            if (entity == null || entity.CreatedSubject?.SubjectId != Guid.Parse(subjectPrincipal.SubjectId)) {
                throw new RecipeNotFoundException($"Recipe with id {recipeResourceId} not found");
            }
            return entity;
        }

        private static PaprikaRecipe MapToPaprikaRecipe(Recipe recipe) {
            return new PaprikaRecipe {
                Name = recipe.Title,
                Ingredients = FormatIngredientsForPaprika(recipe.Ingredients),
                Directions = FormatInstructionsForPaprika(recipe.Instructions),
                Source = recipe.Source,
                SourceUrl = recipe.OriginalImageUrl, // Use OriginalImageUrl as source_url
                Servings = recipe.Yield.ToString(System.Globalization.CultureInfo.InvariantCulture),
                PrepTime = FormatTimeForPaprika(recipe.PrepTimeMinutes),
                CookTime = FormatTimeForPaprika(recipe.CookTimeMinutes),
                Notes = recipe.Description,
                Categories = recipe.RecipeTags?
                    .Where(rt => rt.Tag != null)
                    .Select(rt => rt.Tag.Name)
                    .ToArray() ?? Array.Empty<string>()
            };
        }

        private static string FormatIngredientsForPaprika(IReadOnlyList<RecipeIngredient> ingredients) {
            if (ingredients == null || ingredients.Count == 0) {
                return string.Empty;
            }

            var lines = ingredients
                .OrderBy(i => i.SortOrder)
                .Select(i => {
                    if (!string.IsNullOrWhiteSpace(i.RawText)) {
                        return i.RawText;
                    }
                    
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
                    if (!string.IsNullOrWhiteSpace(i.Preparation)) {
                        parts.Add(i.Preparation);
                    }
                    return string.Join(" ", parts);
                })
                .Where(s => !string.IsNullOrWhiteSpace(s));

            return string.Join("\n", lines);
        }

        private static string FormatInstructionsForPaprika(IReadOnlyList<RecipeInstruction> instructions) {
            if (instructions == null || instructions.Count == 0) {
                return string.Empty;
            }

            var lines = instructions
                .OrderBy(i => i.StepNumber)
                .Select(i => i.Instruction)
                .Where(s => !string.IsNullOrWhiteSpace(s));

            return string.Join("\n", lines);
        }

        private static string FormatTimeForPaprika(int? minutes) {
            if (!minutes.HasValue || minutes.Value == 0) {
                return string.Empty;
            }

            if (minutes.Value < 60) {
                return $"{minutes.Value} min";
            }

            var hours = minutes.Value / 60;
            var mins = minutes.Value % 60;

            if (mins == 0) {
                return $"{hours} hour{(hours > 1 ? "s" : "")}";
            }

            return $"{hours} hour{(hours > 1 ? "s" : "")} {mins} min";
        }

        private static byte[] CompressToGzip(string content) {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress)) {
                var bytes = Encoding.UTF8.GetBytes(content);
                gzip.Write(bytes, 0, bytes.Length);
            }
            return output.ToArray();
        }
    }
}
