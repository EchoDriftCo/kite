using Microsoft.Extensions.Logging;
using Moq;

namespace RecipeVault.Facade.Tests.Base {
    public abstract class FacadeTestBase {
        protected MockRepository MockRepository { get; private set; }

        protected FacadeTestBase() {
            MockRepository = new MockRepository(MockBehavior.Strict);
        }

        protected Mock<ILogger<T>> CreateMockLogger<T>() where T : class {
            return MockRepository.Create<ILogger<T>>();
        }

        protected void VerifyAllMocks() {
            MockRepository.VerifyAll();
        }
    }
}
