#pragma warning disable S3376

using Cortside.Common.Messages.MessageExceptions;

namespace RecipeVault.Exceptions {
    public class MealPlanNotFoundException : NotFoundResponseException {
        public MealPlanNotFoundException(string message) : base(message) {
        }
    }
}
