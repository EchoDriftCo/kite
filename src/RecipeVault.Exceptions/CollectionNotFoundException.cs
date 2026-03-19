#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class CollectionNotFoundException : NotFoundResponseException {
        public CollectionNotFoundException(string message) : base(message) {
        }
    }
}
