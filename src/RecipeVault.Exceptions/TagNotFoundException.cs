#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class TagNotFoundException : NotFoundResponseException {
        public TagNotFoundException(string message) : base(message) {
        }
    }
}
