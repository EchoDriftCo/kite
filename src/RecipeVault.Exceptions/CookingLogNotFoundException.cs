#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class CookingLogNotFoundException : NotFoundResponseException {
        public CookingLogNotFoundException(string message) : base(message) {
        }
    }
}
