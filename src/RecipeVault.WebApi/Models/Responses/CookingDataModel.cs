using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecipeVault.WebApi.Models.Responses {
    public class CookingDataModel {
        [JsonPropertyName("recipeResourceId")]
        public Guid RecipeResourceId { get; set; }
        
        [JsonPropertyName("steps")]
        public List<CookingStepModel> Steps { get; set; }
        
        [JsonPropertyName("timers")]
        public List<TimerModel> Timers { get; set; }
        
        [JsonPropertyName("temperatures")]
        public List<TemperatureModel> Temperatures { get; set; }
    }

    public class CookingStepModel {
        [JsonPropertyName("stepNumber")]
        public int StepNumber { get; set; }
        
        [JsonPropertyName("instruction")]
        public string Instruction { get; set; }
        
        [JsonPropertyName("timerIndexes")]
        public List<int> TimerIndexes { get; set; }
    }

    public class TimerModel {
        [JsonPropertyName("index")]
        public int Index { get; set; }
        
        [JsonPropertyName("label")]
        public string Label { get; set; }
        
        [JsonPropertyName("seconds")]
        public int Seconds { get; set; }
        
        [JsonPropertyName("stepNumber")]
        public int StepNumber { get; set; }
    }

    public class TemperatureModel {
        [JsonPropertyName("stepNumber")]
        public int StepNumber { get; set; }
        
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
        
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
        
        [JsonPropertyName("context")]
        public string Context { get; set; }
    }
}
