using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public class UsdaFoodDataService : IUsdaFoodDataService {
        private readonly HttpClient _httpClient;
        private readonly UsdaConfiguration _config;
        private readonly ILogger<UsdaFoodDataService> _logger;

        public UsdaFoodDataService(
            HttpClient httpClient,
            IOptions<UsdaConfiguration> config,
            ILogger<UsdaFoodDataService> logger) {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            if (!string.IsNullOrEmpty(_config.ApiKey)) {
                _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            }
        }

        public async Task<FoodSearchResponse> SearchFoodsAsync(string query, int pageSize = 10) {
            // For now, use mock data if API key is not configured
            if (string.IsNullOrEmpty(_config.ApiKey)) {
                _logger.LogWarning("USDA API key not configured. Using mock data.");
                return GetMockSearchResponse(query, pageSize);
            }

            try {
                var response = await _httpClient.GetFromJsonAsync<FoodSearchResponse>(
                    $"/foods/search?query={Uri.EscapeDataString(query)}&pageSize={pageSize}&api_key={_config.ApiKey}");
                return response ?? new FoodSearchResponse { Foods = new List<FoodSearchResult>(), TotalHits = 0 };
            } catch (Exception ex) {
                _logger.LogError(ex, "Error searching USDA foods for query: {Query}", query);
                throw new HttpRequestException("Failed to search USDA foods", ex);
            }
        }

        public async Task<FoodDetails> GetFoodDetailsAsync(int fdcId) {
            // For now, use mock data if API key is not configured
            if (string.IsNullOrEmpty(_config.ApiKey)) {
                _logger.LogWarning("USDA API key not configured. Using mock data.");
                return GetMockFoodDetails(fdcId);
            }

            try {
                var response = await _httpClient.GetFromJsonAsync<FoodDetails>(
                    $"/food/{fdcId}?api_key={_config.ApiKey}");
                return response ?? GetMockFoodDetails(fdcId);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error fetching USDA food details for FdcId: {FdcId}", fdcId);
                throw new HttpRequestException($"Failed to fetch USDA food details for {fdcId}", ex);
            }
        }

        public async Task<List<FoodSearchResult>> SearchWithBestMatchAsync(string ingredientText, int maxResults = 5) {
            var searchResponse = await SearchFoodsAsync(ingredientText, maxResults);
            return searchResponse.Foods.Take(maxResults).ToList();
        }

        // Mock data for development/testing
        private FoodSearchResponse GetMockSearchResponse(string query, int pageSize) {
            var lowerQuery = query.ToLower();
            var mockFoods = new List<FoodSearchResult>();

            // Common ingredients mock data
            if (lowerQuery.Contains("chicken")) {
                mockFoods.Add(CreateMockFood(173851, "Chicken, broilers or fryers, meat only, cooked, roasted", 165m, 31m, 0m, 3.6m));
            }
            if (lowerQuery.Contains("rice")) {
                mockFoods.Add(CreateMockFood(168878, "Rice, white, long-grain, regular, cooked", 130m, 2.7m, 28.2m, 0.3m));
            }
            if (lowerQuery.Contains("tomato")) {
                mockFoods.Add(CreateMockFood(170457, "Tomatoes, raw", 18m, 0.9m, 3.9m, 0.2m));
            }
            if (lowerQuery.Contains("onion")) {
                mockFoods.Add(CreateMockFood(170000, "Onions, raw", 40m, 1.1m, 9.3m, 0.1m));
            }
            if (lowerQuery.Contains("garlic")) {
                mockFoods.Add(CreateMockFood(169230, "Garlic, raw", 149m, 6.4m, 33m, 0.5m));
            }
            if (lowerQuery.Contains("olive oil")) {
                mockFoods.Add(CreateMockFood(171413, "Oil, olive, salad or cooking", 884m, 0m, 0m, 100m));
            }
            if (lowerQuery.Contains("flour")) {
                mockFoods.Add(CreateMockFood(169729, "Wheat flour, white, all-purpose", 364m, 10.3m, 76.3m, 1m));
            }
            if (lowerQuery.Contains("sugar")) {
                mockFoods.Add(CreateMockFood(169655, "Sugars, granulated", 387m, 0m, 100m, 0m));
            }
            if (lowerQuery.Contains("butter")) {
                mockFoods.Add(CreateMockFood(172430, "Butter, salted", 717m, 0.9m, 0.1m, 81.1m));
            }
            if (lowerQuery.Contains("egg")) {
                mockFoods.Add(CreateMockFood(173424, "Egg, whole, raw, fresh", 143m, 12.6m, 0.7m, 9.5m));
            }
            if (lowerQuery.Contains("milk")) {
                mockFoods.Add(CreateMockFood(171265, "Milk, whole, 3.25% milkfat", 61m, 3.2m, 4.8m, 3.3m));
            }
            if (lowerQuery.Contains("beef")) {
                mockFoods.Add(CreateMockFood(174032, "Beef, ground, 85% lean meat / 15% fat, cooked", 215m, 26.1m, 0m, 11.3m));
            }
            if (lowerQuery.Contains("potato")) {
                mockFoods.Add(CreateMockFood(170032, "Potatoes, raw, skin", 77m, 2m, 17.5m, 0.1m));
            }
            if (lowerQuery.Contains("carrot")) {
                mockFoods.Add(CreateMockFood(170393, "Carrots, raw", 41m, 0.9m, 9.6m, 0.2m));
            }
            if (lowerQuery.Contains("broccoli")) {
                mockFoods.Add(CreateMockFood(170379, "Broccoli, raw", 34m, 2.8m, 6.6m, 0.4m));
            }
            if (lowerQuery.Contains("cheese")) {
                mockFoods.Add(CreateMockFood(173417, "Cheese, cheddar", 403m, 24.9m, 1.3m, 33.1m));
            }
            if (lowerQuery.Contains("pasta")) {
                mockFoods.Add(CreateMockFood(169734, "Pasta, cooked, enriched, without added salt", 131m, 5m, 25m, 1.1m));
            }
            if (lowerQuery.Contains("bread")) {
                mockFoods.Add(CreateMockFood(172687, "Bread, white, commercially prepared", 265m, 9.4m, 49m, 3.3m));
            }
            if (lowerQuery.Contains("salmon")) {
                mockFoods.Add(CreateMockFood(175168, "Fish, salmon, Atlantic, farmed, cooked", 206m, 22.5m, 0m, 12.4m));
            }
            if (lowerQuery.Contains("apple")) {
                mockFoods.Add(CreateMockFood(171688, "Apples, raw, with skin", 52m, 0.3m, 13.8m, 0.2m));
            }

            // Generic fallback
            if (!mockFoods.Any()) {
                mockFoods.Add(CreateMockFood(999999, $"{query} (mock data)", 100m, 5m, 15m, 2m));
            }

            return new FoodSearchResponse {
                Foods = mockFoods.Take(pageSize).ToList(),
                TotalHits = mockFoods.Count
            };
        }

        private FoodSearchResult CreateMockFood(int fdcId, string description, decimal calories, decimal protein, decimal carbs, decimal fat) {
            return new FoodSearchResult {
                FdcId = fdcId,
                Description = description,
                DataType = "SR Legacy",
                FoodNutrients = new List<FoodNutrient> {
                    new FoodNutrient { NutrientId = 1008, NutrientName = "Energy", NutrientNumber = "208", UnitName = "KCAL", Value = calories },
                    new FoodNutrient { NutrientId = 1003, NutrientName = "Protein", NutrientNumber = "203", UnitName = "G", Value = protein },
                    new FoodNutrient { NutrientId = 1005, NutrientName = "Carbohydrate, by difference", NutrientNumber = "205", UnitName = "G", Value = carbs },
                    new FoodNutrient { NutrientId = 1004, NutrientName = "Total lipid (fat)", NutrientNumber = "204", UnitName = "G", Value = fat }
                }
            };
        }

        private FoodDetails GetMockFoodDetails(int fdcId) {
            return new FoodDetails {
                FdcId = fdcId,
                Description = $"Mock food {fdcId}",
                DataType = "SR Legacy",
                FoodNutrients = new List<DetailedFoodNutrient> {
                    new DetailedFoodNutrient {
                        Nutrient = new Nutrient { Id = 1008, Name = "Energy", Number = "208", UnitName = "KCAL" },
                        Amount = 100
                    },
                    new DetailedFoodNutrient {
                        Nutrient = new Nutrient { Id = 1003, Name = "Protein", Number = "203", UnitName = "G" },
                        Amount = 5
                    }
                },
                FoodPortions = new List<FoodPortion> {
                    new FoodPortion {
                        Id = 1,
                        Amount = 1,
                        GramWeight = 100,
                        MeasureUnit = new MeasureUnit { Id = 1, Abbreviation = "cup", Name = "cup" }
                    }
                }
            };
        }
    }
}
