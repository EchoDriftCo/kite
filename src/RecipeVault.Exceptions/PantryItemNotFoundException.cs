#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class PantryItemNotFoundException : NotFoundResponseException {
        public PantryItemNotFoundException(string message) : base(message) {
        }
    }
}
