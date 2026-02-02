using Moq;

namespace RecipeVault.WebApi.Tests.Base {
    public abstract class ControllerTestBase {
        protected MockRepository MockRepository { get; private set; }

        protected ControllerTestBase() {
            MockRepository = new MockRepository(MockBehavior.Strict);
        }

        protected void VerifyAllMocks() {
            MockRepository.VerifyAll();
        }
    }
}
