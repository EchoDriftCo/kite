using Microsoft.Extensions.Logging;
using Moq;

namespace RecipeVault.DomainService.Tests.Base {
    public abstract class DomainServiceTestBase {
        protected MockRepository MockRepository { get; private set; }

        protected DomainServiceTestBase() {
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
