#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class ApiTokenNotFoundException : NotFoundResponseException {
        public ApiTokenNotFoundException(string message) : base(message) {
        }
    }
}
