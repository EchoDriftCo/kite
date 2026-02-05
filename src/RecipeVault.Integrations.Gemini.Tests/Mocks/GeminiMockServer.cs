using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace RecipeVault.Integrations.Gemini.Tests.Mocks {
    /// <summary>
    /// WireMock server for Gemini API testing.
    /// Can be used standalone or configured through IMockServer pattern.
    /// </summary>
    public class GeminiMockServer : IMockServer, IDisposable {
        private WireMockServer server;
        private readonly bool ownsServer;
        private static readonly JsonSerializerOptions JsonOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Creates a standalone mock server
        /// </summary>
        public GeminiMockServer() {
            server = WireMockServer.Start();
            ownsServer = true;
        }

        /// <summary>
        /// Creates a mock configured on an existing server
        /// </summary>
        public GeminiMockServer(WireMockServer existingServer) {
            server = existingServer ?? throw new ArgumentNullException(nameof(existingServer));
            ownsServer = false;
        }

        /// <summary>
        /// Gets the base URL of the mock server
        /// </summary>
        public string Url => server?.Url;

        /// <summary>
        /// Gets the port the mock server is running on
        /// </summary>
        public int Port => server?.Port ?? 0;

        /// <summary>
        /// Gets the underlying WireMock server
        /// </summary>
        public WireMockServer WireMockServer => server;

        /// <summary>
        /// Configure the mock server with default stubs (IMockServer pattern)
        /// </summary>
        public void Configure(WireMockServer server) {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            StubParseRecipeSuccess();
        }

        /// <summary>
        /// Stub a successful recipe parse response
        /// </summary>
        public GeminiMockServer StubParseRecipeSuccess(
            string title = "Chocolate Cake",
            int? yield = 8,
            int? prepTimeMinutes = 15,
            int? cookTimeMinutes = 30,
            List<MockIngredient> ingredients = null,
            List<MockInstruction> instructions = null) {

            ingredients ??= new List<MockIngredient> {
                new() { Quantity = 2, Unit = "cup", Item = "flour", Preparation = null, RawText = "2 cups flour" },
                new() { Quantity = 1, Unit = "cup", Item = "sugar", Preparation = null, RawText = "1 cup sugar" }
            };

            instructions ??= new List<MockInstruction> {
                new() { StepNumber = 1, Instruction = "Preheat oven to 350°F", RawText = "Preheat oven to 350°F" },
                new() { StepNumber = 2, Instruction = "Mix dry ingredients", RawText = "Mix dry ingredients" }
            };

            var recipeResult = new {
                title,
                yield,
                prepTimeMinutes,
                cookTimeMinutes,
                ingredients,
                instructions
            };

            var recipeJson = JsonSerializer.Serialize(recipeResult, JsonOptions);

            var response = new {
                candidates = new[] {
                    new {
                        content = new {
                            parts = new[] {
                                new { text = recipeJson }
                            }
                        },
                        finishReason = "STOP"
                    }
                },
                usageMetadata = new {
                    promptTokenCount = 100,
                    candidatesTokenCount = 200,
                    totalTokenCount = 300
                }
            };

            server
                .Given(Request.Create()
                    .WithPath("/v1beta/models/*")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(response, JsonOptions)));

            return this;
        }

        /// <summary>
        /// Stub a response with missing/null fields to test warnings
        /// </summary>
        public GeminiMockServer StubParseRecipeWithMissingFields(string title = "Chocolate Cake") {
            var recipeResult = new {
                title,
                yield = (int?)null,
                prepTimeMinutes = (int?)null,
                cookTimeMinutes = (int?)null,
                ingredients = new List<MockIngredient>(),
                instructions = new List<MockInstruction>()
            };

            var recipeJson = JsonSerializer.Serialize(recipeResult, JsonOptions);

            var response = new {
                candidates = new[] {
                    new {
                        content = new {
                            parts = new[] {
                                new { text = recipeJson }
                            }
                        },
                        finishReason = "STOP"
                    }
                }
            };

            server
                .Given(Request.Create()
                    .WithPath("/v1beta/models/*")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(response, JsonOptions)));

            return this;
        }

        /// <summary>
        /// Stub a 422 Unprocessable Entity response (no recipe detected)
        /// </summary>
        public GeminiMockServer StubNoRecipeDetected() {
            var errorResponse = new {
                error = new {
                    code = 422,
                    message = "Could not identify a recipe in the image",
                    details = new List<object>()
                }
            };

            server
                .Given(Request.Create()
                    .WithPath("/v1beta/models/*")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(422)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(errorResponse, JsonOptions)));

            return this;
        }

        /// <summary>
        /// Stub a 503 Service Unavailable response (API down)
        /// </summary>
        public GeminiMockServer StubServiceUnavailable() {
            var errorResponse = new {
                error = new {
                    code = 503,
                    message = "Service temporarily unavailable",
                    details = new List<object>()
                }
            };

            server
                .Given(Request.Create()
                    .WithPath("/v1beta/models/*")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(503)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(errorResponse, JsonOptions)));

            return this;
        }

        /// <summary>
        /// Stub a 401 Unauthorized response (invalid API key)
        /// </summary>
        public GeminiMockServer StubUnauthorized() {
            var errorResponse = new {
                error = new {
                    code = 401,
                    message = "Invalid API key",
                    details = new List<object>()
                }
            };

            server
                .Given(Request.Create()
                    .WithPath("/v1beta/models/*")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(401)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(errorResponse, JsonOptions)));

            return this;
        }

        /// <summary>
        /// Stub a response with a delay to test timeout handling
        /// </summary>
        public GeminiMockServer StubWithDelay(TimeSpan delay) {
            var recipeResult = new {
                title = "Slow Recipe",
                yield = 4,
                prepTimeMinutes = 10,
                cookTimeMinutes = 20,
                ingredients = new List<MockIngredient>(),
                instructions = new List<MockInstruction>()
            };

            var recipeJson = JsonSerializer.Serialize(recipeResult, JsonOptions);

            var response = new {
                candidates = new[] {
                    new {
                        content = new {
                            parts = new[] {
                                new { text = recipeJson }
                            }
                        }
                    }
                }
            };

            server
                .Given(Request.Create()
                    .WithPath("/v1beta/models/*")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(response, JsonOptions))
                    .WithDelay(delay));

            return this;
        }

        /// <summary>
        /// Reset all stubs
        /// </summary>
        public GeminiMockServer Reset() {
            server?.Reset();
            return this;
        }

        public void Dispose() {
            if (ownsServer) {
                server?.Stop();
                server?.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Mock ingredient for test data
    /// </summary>
    public class MockIngredient {
        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("preparation")]
        public string Preparation { get; set; }

        [JsonPropertyName("rawText")]
        public string RawText { get; set; }
    }

    /// <summary>
    /// Mock instruction for test data
    /// </summary>
    public class MockInstruction {
        [JsonPropertyName("stepNumber")]
        public int StepNumber { get; set; }

        [JsonPropertyName("instruction")]
        public string Instruction { get; set; }

        [JsonPropertyName("rawText")]
        public string RawText { get; set; }
    }
}
