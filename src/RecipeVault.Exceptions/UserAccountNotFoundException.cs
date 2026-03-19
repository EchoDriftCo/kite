#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class UserAccountNotFoundException : NotFoundResponseException {
        public UserAccountNotFoundException(string message) : base(message) {
        }
    }
}
