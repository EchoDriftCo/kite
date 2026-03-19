using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace RecipeVault.DomainService.Tests.Base {
    public abstract class DomainServiceTestBase {
        protected MockRepository MockRepository { get; private set; }

        protected DomainServiceTestBase() {
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

            mockLogger
                .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
                .Returns(new NullDisposable());
        }

        protected void VerifyAllMocks() {
            MockRepository.VerifyAll();
        }

        /// <summary>
        /// Null disposable for BeginScope mock
        /// </summary>
        private sealed class NullDisposable : IDisposable {
            public void Dispose() {
                // No-op
            }
        }
    }
}
