using System;
using System.Collections.Generic;
using RecipeVault.Domain.Entities;

namespace RecipeVault.TestUtilities.Builders {
    public class MealPlanBuilder {
        private string _name = "Test Meal Plan";
        private DateTime _startDate = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        private DateTime _endDate = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc);
        private List<MealPlanEntry> _entries = new();

        public MealPlanBuilder WithName(string name) {
            _name = name;
            return this;
        }

        public MealPlanBuilder WithStartDate(DateTime startDate) {
            _startDate = startDate;
            return this;
        }

        public MealPlanBuilder WithEndDate(DateTime endDate) {
            _endDate = endDate;
            return this;
        }

        public MealPlanBuilder WithEntries(List<MealPlanEntry> entries) {
            _entries = entries;
            return this;
        }

        public MealPlan Build() {
            var mealPlan = new MealPlan(_name, _startDate, _endDate);
            if (_entries.Count > 0) {
                mealPlan.SetEntries(_entries);
            }
            return mealPlan;
        }
    }
}
