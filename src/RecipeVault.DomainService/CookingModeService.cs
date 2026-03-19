using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public class CookingModeService : ICookingModeService {
        // Timer patterns: "for X minutes", "X-Y minutes", "X hours", etc.
        private static readonly Regex TimerPattern = new Regex(
            @"(?:for\s+)?(?:about\s+)?(\d+)(?:\s*-\s*(\d+))?\s*(minute|min|hour|hr)s?(?:\s+(?:or\s+)?until)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        // Temperature patterns: "350°F", "180 degrees C", etc.
        private static readonly Regex TempPattern = new Regex(
            @"(\d+)\s*(?:°|degree|degrees)?\s*([FC])\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public Task<CookingDataDto> GetCookingDataAsync(Recipe recipe) {
            var steps = new List<CookingStepDto>();
            var timers = new List<TimerDto>();
            var temperatures = new List<TemperatureDto>();
            int timerIndex = 0;

            foreach (var instruction in recipe.Instructions.OrderBy(i => i.StepNumber)) {
                var stepNumber = instruction.StepNumber;
                var text = instruction.Instruction;
                var stepTimerIndexes = new List<int>();

                // Extract timers from this step
                var timerMatches = TimerPattern.Matches(text);
                foreach (Match match in timerMatches) {
                    var value = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    var maxValue = match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : value;
                    var unit = match.Groups[3].Value.ToLowerInvariant();

                    int seconds;
                    if (unit.Length > 0 && unit[0] == 'h') {
                        seconds = value * 3600;
                    } else {
                        seconds = value * 60;
                    }

                    // Generate a label for the timer
                    var label = GenerateTimerLabel(text, value, maxValue, unit);

                    timers.Add(new TimerDto {
                        Index = timerIndex,
                        Label = label,
                        Seconds = seconds,
                        StepNumber = stepNumber
                    });

                    stepTimerIndexes.Add(timerIndex);
                    timerIndex++;
                }

                // Extract temperatures from this step
                var tempMatches = TempPattern.Matches(text);
                foreach (Match match in tempMatches) {
                    var value = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    var unit = match.Groups[2].Value.ToUpperInvariant();
                    var context = ExtractTemperatureContext(text, match.Index);

                    temperatures.Add(new TemperatureDto {
                        StepNumber = stepNumber,
                        Value = value,
                        Unit = unit,
                        Context = context
                    });
                }

                steps.Add(new CookingStepDto {
                    StepNumber = stepNumber,
                    Instruction = text,
                    TimerIndexes = stepTimerIndexes
                });
            }

            return Task.FromResult(new CookingDataDto {
                RecipeResourceId = recipe.RecipeResourceId,
                Steps = steps,
                Timers = timers,
                Temperatures = temperatures
            });
        }

        private static string GenerateTimerLabel(string instruction, int value, int maxValue, string unit) {
            // Extract action from beginning of instruction (bake, rest, cook, etc.)
            var words = instruction.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var action = words.Length > 0 ? words[0] : "Timer";

            // Capitalize first letter
            action = char.ToUpperInvariant(action[0]) + action.Substring(1).ToLowerInvariant();

            // Format time
            string timeStr;
            if (value == maxValue) {
                timeStr = $"{value} {unit}";
            } else {
                timeStr = $"{value}-{maxValue} {unit}";
            }

            return $"{action} ({timeStr})";
        }

        private static string ExtractTemperatureContext(string text, int tempPosition) {
            // Look for context words near the temperature
            var lowerText = text.ToLowerInvariant();
            
            if (lowerText.Contains("oven")) return "oven";
            if (lowerText.Contains("internal") || lowerText.Contains("thermometer")) return "internal temp";
            if (lowerText.Contains("grill")) return "grill";
            if (lowerText.Contains("fryer")) return "fryer";
            if (lowerText.Contains("water") || lowerText.Contains("boil")) return "water";
            
            return "temperature";
        }
    }
}
