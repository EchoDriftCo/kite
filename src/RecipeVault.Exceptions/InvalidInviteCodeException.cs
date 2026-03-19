#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class InvalidInviteCodeException : BadRequestResponseException {
        public InvalidInviteCodeException(string message) : base(message) {
        }
    }
}
