using System;
using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class CookingDataModel {
        public Guid RecipeResourceId { get; set; }
        public List<CookingStepModel> Steps { get; set; }
        public List<TimerModel> Timers { get; set; }
        public List<TemperatureModel> Temperatures { get; set; }
    }

    public class CookingStepModel {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public List<int> TimerIndexes { get; set; }
    }

    public class TimerModel {
        public int Index { get; set; }
        public string Label { get; set; }
        public int Seconds { get; set; }
        public int StepNumber { get; set; }
    }

    public class TemperatureModel {
        public int StepNumber { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public string Context { get; set; }
    }
}
