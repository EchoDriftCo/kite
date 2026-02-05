using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.DomainService {
    public interface IUnitService {
        Task<UnitMatchResult> MatchAsync(string input);
        Task<IReadOnlyList<Unit>> GetAllAsync();
        Task<Unit> GetByIdAsync(Guid resourceId);
    }

    public class UnitMatchResult {
        public bool IsMatch { get; set; }
        public Unit MatchedUnit { get; set; }
        public decimal Confidence { get; set; }
        public string OriginalInput { get; set; }

        public static UnitMatchResult NoMatch(string input) => new UnitMatchResult {
            IsMatch = false,
            OriginalInput = input,
            Confidence = 0
        };

        public static UnitMatchResult ExactMatch(Unit unit, string input) => new UnitMatchResult {
            IsMatch = true,
            MatchedUnit = unit,
            Confidence = 1.0m,
            OriginalInput = input
        };

        public static UnitMatchResult FuzzyMatch(Unit unit, string input, decimal confidence) => new UnitMatchResult {
            IsMatch = true,
            MatchedUnit = unit,
            Confidence = confidence,
            OriginalInput = input
        };
    }
}
