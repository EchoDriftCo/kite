#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class CircleInviteNotFoundException : NotFoundResponseException {
        public CircleInviteNotFoundException(string message) : base(message) {
        }
    }
}
