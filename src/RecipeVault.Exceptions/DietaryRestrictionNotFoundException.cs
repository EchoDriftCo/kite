using System;

namespace RecipeVault.Exceptions {
    public class DietaryRestrictionNotFoundException : Exception {
        public DietaryRestrictionNotFoundException() {
        }

        public DietaryRestrictionNotFoundException(string message) : base(message) {
        }

        public DietaryRestrictionNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
