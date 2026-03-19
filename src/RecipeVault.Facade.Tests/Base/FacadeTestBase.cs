using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace RecipeVault.Facade.Tests.Base {
    public abstract class FacadeTestBase {
        protected MockRepository MockRepository { get; private set; }

        protected FacadeTestBase() {
            MockRepository = new MockRepository(MockBehavior.Strict);
        }

        protected Mock<ILogger<T>> CreateMockLogger<T>() where T : class {
            var mockLogger = MockRepository.Create<ILogger<T>>();
            SetupLoggerMock(mockLogger);
            return mockLogger;
        }

        protected static void SetupLoggerMock<T>(Mock<ILogger<T>> mockLogger) where T : class {
            mockLogger
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();
        }

        protected void VerifyAllMocks() {
            MockRepository.VerifyAll();
        }
    }
}
