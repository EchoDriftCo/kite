#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class InviteCodeExpiredException : BadRequestResponseException {
        public InviteCodeExpiredException(string message) : base(message) {
        }
    }
}
