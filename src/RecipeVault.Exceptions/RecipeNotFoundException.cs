#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class RecipeNotFoundException : NotFoundResponseException {
        public RecipeNotFoundException(string message) : base(message) {
        }
    }
}
