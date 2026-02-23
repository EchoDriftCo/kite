using Shouldly;
using Xunit;
using RecipeVault.DomainService.Utilities;

namespace RecipeVault.DomainService.Tests.Utilities {
    public class Iso8601DurationParserTests {
        [Theory]
        [InlineData("PT30M", 30)]
        [InlineData("PT45M", 45)]
        [InlineData("PT1H", 60)]
        [InlineData("PT1H30M", 90)]
        [InlineData("PT2H15M", 135)]
        [InlineData("PT15S", 1)]  // 15 seconds rounds up to 1 minute
        [InlineData("PT90S", 2)]  // 90 seconds = 1.5 minutes, rounds up to 2
        [InlineData("PT1H15M30S", 76)] // 1h 15m 30s = 75.5 minutes, rounds up to 76
        [InlineData("P1D", 1440)]  // 1 day = 1440 minutes
        [InlineData("P1DT2H30M", 1590)]  // 1 day + 2.5 hours = 1590 minutes
        public void ParseToMinutes_WithValidDuration_ReturnsCorrectMinutes(string duration, int expectedMinutes) {
            // Act
            var result = Iso8601DurationParser.ParseToMinutes(duration);

            // Assert
            result.ShouldBe(expectedMinutes);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid")]
        [InlineData("30 minutes")]  // Not ISO 8601
        [InlineData("1 hour")]      // Not ISO 8601
        [InlineData("PT")]          // No values
        [InlineData("P")]           // No values
        public void ParseToMinutes_WithInvalidDuration_ReturnsNull(string duration) {
            // Act
            var result = Iso8601DurationParser.ParseToMinutes(duration);

            // Assert
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData("pt30m", 30)]   // Lowercase
        [InlineData("Pt1H30M", 90)] // Mixed case
        [InlineData("PT1h30m", 90)] // Lowercase units
        public void ParseToMinutes_IsCaseInsensitive(string duration, int expectedMinutes) {
            // Act
            var result = Iso8601DurationParser.ParseToMinutes(duration);

            // Assert
            result.ShouldBe(expectedMinutes);
        }

        [Theory]
        [InlineData("  PT30M  ", 30)]   // Leading/trailing whitespace
        public void ParseToMinutes_TrimsWhitespace(string duration, int expectedMinutes) {
            // Act
            var result = Iso8601DurationParser.ParseToMinutes(duration);

            // Assert
            result.ShouldBe(expectedMinutes);
        }
    }
}
