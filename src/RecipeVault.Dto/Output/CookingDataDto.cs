using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class CookingDataDto {
        public Guid RecipeResourceId { get; set; }
        public List<CookingStepDto> Steps { get; set; }
        public List<TimerDto> Timers { get; set; }
        public List<TemperatureDto> Temperatures { get; set; }
    }

    public class CookingStepDto {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
        public List<int> TimerIndexes { get; set; } // References to timers extracted from this step
    }

    public class TimerDto {
        public int Index { get; set; }
        public string Label { get; set; }
        public int Seconds { get; set; }
        public int StepNumber { get; set; }
    }

    public class TemperatureDto {
        public int StepNumber { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; } // "F" or "C"
        public string Context { get; set; } // e.g., "oven", "internal temp"
    }
}
