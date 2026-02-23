#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class CircleNotFoundException : NotFoundResponseException {
        public CircleNotFoundException(string message) : base(message) {
        }
    }
}
