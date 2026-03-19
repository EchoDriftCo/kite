using System;

namespace RecipeVault.Exceptions {
    public class DietaryProfileNotFoundException : Exception {
        public DietaryProfileNotFoundException() {
        }

        public DietaryProfileNotFoundException(string message) : base(message) {
        }

        public DietaryProfileNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
