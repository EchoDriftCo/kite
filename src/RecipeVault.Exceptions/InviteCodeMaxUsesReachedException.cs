#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class InviteCodeMaxUsesReachedException : BadRequestResponseException {
        public InviteCodeMaxUsesReachedException(string message) : base(message) {
        }
    }
}
