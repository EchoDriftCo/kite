using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipeVault.DomainService.Models {
    /// <summary>
    /// Handles recipeYield which can be string, number, or array
    /// </summary>
    public class FlexibleYieldConverter : JsonConverter<int?> {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number) {
                return reader.GetInt32();
            }

            if (reader.TokenType == JsonTokenType.String) {
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text)) {
                    return null;
                }

                // Try to extract first number from string like "4 servings", "Serves 6", etc.
                var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
                if (match.Success && int.TryParse(match.Value, out var result)) {
                    return result;
                }
                return null;
            }

            // Handle arrays - take first element
            if (reader.TokenType == JsonTokenType.StartArray) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.Number) {
                    var value = reader.GetInt32();
                    // Skip to end of array
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) { }
                    return value;
                }
                if (reader.TokenType == JsonTokenType.String) {
                    var text = reader.GetString();
                    // Skip to end of array
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) { }
                    
                    if (string.IsNullOrWhiteSpace(text)) {
                        return null;
                    }
                    var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
                    if (match.Success && int.TryParse(match.Value, out var result)) {
                        return result;
                    }
                }
                // Skip to end of array if not already there
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) { }
                return null;
            }

            return null;
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
    /// Handles recipeInstructions which can be string, array of strings, or array of HowToStep objects
    /// </summary>
    public class FlexibleInstructionsConverter : JsonConverter<string[]> {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) {
                return Array.Empty<string>();
            }

            if (reader.TokenType == JsonTokenType.String) {
                return new[] { reader.GetString() };
            }

            if (reader.TokenType == JsonTokenType.StartArray) {
                var instructions = new System.Collections.Generic.List<string>();
                
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                    if (reader.TokenType == JsonTokenType.String) {
                        instructions.Add(reader.GetString());
                    } else if (reader.TokenType == JsonTokenType.StartObject) {
                        // Parse HowToStep object
                        string text = null;
                        
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                            if (reader.TokenType == JsonTokenType.PropertyName) {
                                var propertyName = reader.GetString();
                                reader.Read();
                                
                                if (propertyName == "text" && reader.TokenType == JsonTokenType.String) {
                                    text = reader.GetString();
                                }
                            }
                        }
                        
                        if (!string.IsNullOrWhiteSpace(text)) {
                            instructions.Add(text);
                        }
                    }
                }
                
                return instructions.ToArray();
            }

            return Array.Empty<string>();
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            foreach (var item in value) {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Handles image which can be string, array, or object with url property
    /// </summary>
    public class FlexibleImageConverter : JsonConverter<string> {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String) {
                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.StartArray) {
                // Take first image from array
                reader.Read();
                string result = null;
                
                if (reader.TokenType == JsonTokenType.String) {
                    result = reader.GetString();
                } else if (reader.TokenType == JsonTokenType.StartObject) {
                    result = ReadImageObject(ref reader);
                }
                
                // Skip to end of array
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) { }
                
                return result;
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                return ReadImageObject(ref reader);
            }

            return null;
        }

        private static string ReadImageObject(ref Utf8JsonReader reader) {
            string url = null;
            
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    var propertyName = reader.GetString();
                    reader.Read();
                    
                    if (propertyName == "url" && reader.TokenType == JsonTokenType.String) {
                        url = reader.GetString();
                    }
                }
            }
            
            return url;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) {
            if (value != null) {
                writer.WriteStringValue(value);
            } else {
                writer.WriteNullValue();
            }
        }
    }

    /// <summary>
    /// Handles author which can be string, object with name property, or array
    /// </summary>
    public class FlexibleAuthorConverter : JsonConverter<string> {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String) {
                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                return ReadAuthorObject(ref reader);
            }

            if (reader.TokenType == JsonTokenType.StartArray) {
                // Take first author from array
                reader.Read();
                string result = null;
                
                if (reader.TokenType == JsonTokenType.String) {
                    result = reader.GetString();
                } else if (reader.TokenType == JsonTokenType.StartObject) {
                    result = ReadAuthorObject(ref reader);
                }
                
                // Skip to end of array
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) { }
                
                return result;
            }

            return null;
        }

        private static string ReadAuthorObject(ref Utf8JsonReader reader) {
            string name = null;
            
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    var propertyName = reader.GetString();
                    reader.Read();
                    
                    if (propertyName == "name" && reader.TokenType == JsonTokenType.String) {
                        name = reader.GetString();
                    }
                }
            }
            
            return name;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) {
            if (value != null) {
                writer.WriteStringValue(value);
            } else {
                writer.WriteNullValue();
            }
        }
    }
}
