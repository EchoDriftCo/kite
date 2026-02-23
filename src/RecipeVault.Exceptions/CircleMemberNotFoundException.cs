#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class CircleMemberNotFoundException : NotFoundResponseException {
        public CircleMemberNotFoundException(string message) : base(message) {
        }
    }
}
