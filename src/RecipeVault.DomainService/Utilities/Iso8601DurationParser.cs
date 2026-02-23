using System;
using System.Text.RegularExpressions;

namespace RecipeVault.DomainService.Utilities {
    /// <summary>
    /// Parses ISO 8601 duration strings (e.g., PT30M, PT1H30M) to minutes
    /// </summary>
    public static class Iso8601DurationParser {
        // ISO 8601 duration format: P[n]Y[n]M[n]DT[n]H[n]M[n]S
        // For recipes, we typically only see PT[n]H[n]M format
        // Examples: PT30M = 30 minutes, PT1H = 60 minutes, PT1H30M = 90 minutes
        private static readonly Regex DurationRegex = new Regex(
            @"^P(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)D)?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Parse ISO 8601 duration to minutes
        /// </summary>
        /// <param name="duration">ISO 8601 duration string (e.g., PT30M, PT1H30M)</param>
        /// <returns>Total minutes, or null if parsing fails</returns>
        public static int? ParseToMinutes(string duration) {
            if (string.IsNullOrWhiteSpace(duration)) {
                return null;
            }

            var match = DurationRegex.Match(duration.Trim());
            if (!match.Success) {
                return null;
            }

            try {
                var years = ParseGroup(match.Groups[1]);
                var months = ParseGroup(match.Groups[2]);
                var days = ParseGroup(match.Groups[3]);
                var hours = ParseGroup(match.Groups[4]);
                var minutes = ParseGroup(match.Groups[5]);
                var seconds = ParseDecimalGroup(match.Groups[6]);

                // Convert everything to minutes
                // For simplicity, we approximate: 1 year = 365 days, 1 month = 30 days
                var totalMinutes = 0;
                
                if (years > 0) {
                    totalMinutes += years * 365 * 24 * 60; // Unlikely for recipes, but handle it
                }
                
                if (months > 0) {
                    totalMinutes += months * 30 * 24 * 60; // Also unlikely
                }
                
                if (days > 0) {
                    totalMinutes += days * 24 * 60;
                }
                
                if (hours > 0) {
                    totalMinutes += hours * 60;
                }
                
                totalMinutes += minutes;
                
                // Round up seconds to nearest minute
                if (seconds > 0) {
                    totalMinutes += (int)Math.Ceiling(seconds / 60.0);
                }

                return totalMinutes > 0 ? totalMinutes : null;
            }
            catch {
                return null;
            }
        }

        private static int ParseGroup(Group group) {
            if (!group.Success || string.IsNullOrEmpty(group.Value)) {
                return 0;
            }

            return int.TryParse(group.Value, out var result) ? result : 0;
        }

        private static double ParseDecimalGroup(Group group) {
            if (!group.Success || string.IsNullOrEmpty(group.Value)) {
                return 0;
            }

            return double.TryParse(group.Value, out var result) ? result : 0;
        }
    }
}
