using System.Text.Json;
using RecipeVault.Integrations.Gemini.Models;
using Xunit;

namespace RecipeVault.Integrations.Gemini.Tests {
    public class FlexibleConverterTests {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Fact]
        public void CookTimeMinutes_AsInteger_DeserializesCorrectly() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":30}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(30, result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_AsStringInteger_DeserializesCorrectly() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":""30""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(30, result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_AsStringWithUnit_ExtractsNumber() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":""30 min""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(30, result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_AsRange_ExtractsFirstNumber() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":""30-45""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(30, result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_AsDecimal_RoundsToInt() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":10.5}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(10, result.CookTimeMinutes); // 10.5 rounds to nearest even (10)
        }

        [Fact]
        public void CookTimeMinutes_AsDecimalWhole_ReturnsInt() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":30.0}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(30, result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_AsNull_ReturnsNull() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":null}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Null(result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_Missing_ReturnsNull() {
            var json = @"{""title"":""Test""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Null(result.CookTimeMinutes);
        }

        [Fact]
        public void CookTimeMinutes_EmptyString_ReturnsNull() {
            var json = @"{""title"":""Test"",""cookTimeMinutes"":""""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Null(result.CookTimeMinutes);
        }

        [Fact]
        public void Yield_AsString_DeserializesCorrectly() {
            var json = @"{""title"":""Test"",""yield"":""4""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(4, result.Yield);
        }

        [Fact]
        public void Yield_AsStringWithText_ExtractsNumber() {
            var json = @"{""title"":""Test"",""yield"":""4 servings""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);
            Assert.Equal(4, result.Yield);
        }

        [Fact]
        public void Quantity_AsString_DeserializesCorrectly() {
            var json = @"{""item"":""flour"",""quantity"":""2.5""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeIngredient>(json, _options);
            Assert.Equal(2.5m, result.Quantity);
        }

        [Fact]
        public void Quantity_AsStringWithUnit_ExtractsNumber() {
            var json = @"{""item"":""flour"",""quantity"":""2.5 cups""}";
            var result = JsonSerializer.Deserialize<GeminiRecipeIngredient>(json, _options);
            Assert.Equal(2.5m, result.Quantity);
        }

        [Fact]
        public void FullRecipeResponse_WithMixedTypes_DeserializesCorrectly() {
            var json = @"{
                ""title"": ""Chocolate Cake"",
                ""yield"": ""12 servings"",
                ""prepTimeMinutes"": 20,
                ""cookTimeMinutes"": ""35 minutes"",
                ""ingredients"": [
                    {""item"": ""flour"", ""quantity"": ""2.5"", ""unit"": ""cups""},
                    {""item"": ""sugar"", ""quantity"": 1, ""unit"": ""cup""}
                ],
                ""instructions"": [
                    {""stepNumber"": 1, ""instruction"": ""Mix dry ingredients""}
                ]
            }";

            var result = JsonSerializer.Deserialize<GeminiRecipeParseResult>(json, _options);

            Assert.Equal("Chocolate Cake", result.Title);
            Assert.Equal(12, result.Yield);
            Assert.Equal(20, result.PrepTimeMinutes);
            Assert.Equal(35, result.CookTimeMinutes);
            Assert.Equal(2, result.Ingredients.Count);
            Assert.Equal(2.5m, result.Ingredients[0].Quantity);
            Assert.Equal(1, result.Ingredients[1].Quantity);
            Assert.Single(result.Instructions);
        }
    }
}
