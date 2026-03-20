using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RecipeVault.Integrations.Gemini.Converters {
    /// <summary>
    /// JSON converter that handles Gemini API returning integers as strings,
    /// ranges (e.g., "30-45"), or text with units (e.g., "30 min").
    /// Extracts the first integer found and converts to int?.
    /// </summary>
    public class FlexibleNullableIntConverter : JsonConverter<int?> {
        private static readonly Regex NumberPattern = new Regex(@"(\d+)", RegexOptions.Compiled);

        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            switch (reader.TokenType) {
                case JsonTokenType.Number:
                    return reader.GetInt32();

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue)) {
                        return null;
                    }

                    // Try exact int parse first
                    if (int.TryParse(stringValue, out var exactValue)) {
                        return exactValue;
                    }

                    // Extract first number from string (handles "30 min", "30-45", etc.)
                    var match = NumberPattern.Match(stringValue);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var extractedValue)) {
                        return extractedValue;
                    }

                    return null;

                default:
                    // Skip unknown token types (arrays, objects, etc.)
                    reader.Skip();
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options) {
            if (value.HasValue) {
                writer.WriteNumberValue(value.Value);
            } else {
                writer.WriteNullValue();
            }
        }
    }

    /// <summary>
    /// JSON converter for non-nullable decimal that handles string values from Gemini.
    /// </summary>
    public class FlexibleNullableDecimalConverter : JsonConverter<decimal?> {
        private static readonly Regex NumberPattern = new Regex(@"([\d.]+)", RegexOptions.Compiled);

        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            switch (reader.TokenType) {
                case JsonTokenType.Number:
                    return reader.GetDecimal();

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue)) {
                        return null;
                    }

                    if (decimal.TryParse(stringValue, out var exactValue)) {
                        return exactValue;
                    }

                    var match = NumberPattern.Match(stringValue);
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var extractedValue)) {
                        return extractedValue;
                    }

                    return null;

                default:
                    reader.Skip();
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options) {
            if (value.HasValue) {
                writer.WriteNumberValue(value.Value);
            } else {
                writer.WriteNullValue();
            }
        }
    }
}
