using System;

namespace RecipeVault.Exceptions {
    public class EquipmentNotFoundException : Exception {
        public EquipmentNotFoundException() { }

        public EquipmentNotFoundException(string message) : base(message) { }

        public EquipmentNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
