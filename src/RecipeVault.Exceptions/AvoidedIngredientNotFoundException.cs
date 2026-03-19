using System;

namespace RecipeVault.Exceptions {
    public class AvoidedIngredientNotFoundException : Exception {
        public AvoidedIngredientNotFoundException() {
        }

        public AvoidedIngredientNotFoundException(string message) : base(message) {
        }

        public AvoidedIngredientNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
