#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Threading;

namespace RecipeVault.WebApi.IntegrationTests.Helpers {
    /// <summary>
    /// Simple in-memory distributed lock provider for integration tests.
    /// Uses a concurrent dictionary to simulate locking behavior.
    /// </summary>
    public sealed class InMemoryDistributedLockProvider : IDistributedLockProvider {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public IDistributedLock CreateLock(string name) {
            return new InMemoryDistributedLock(name, _locks);
        }
    }

    internal sealed class InMemoryDistributedLock : IDistributedLock {
        private readonly string _name;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

        public InMemoryDistributedLock(string name, ConcurrentDictionary<string, SemaphoreSlim> locks) {
            _name = name;
            _locks = locks;
        }

        public string Name => _name;

        public IDistributedSynchronizationHandle Acquire(TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
            var semaphore = _locks.GetOrAdd(_name, _ => new SemaphoreSlim(1, 1));
            var acquired = semaphore.Wait(timeout ?? TimeSpan.FromSeconds(30), cancellationToken);
            if (!acquired) {
                throw new TimeoutException($"Failed to acquire lock '{_name}'");
            }
            return new InMemoryLockHandle(semaphore);
        }

        public async ValueTask<IDistributedSynchronizationHandle> AcquireAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
            var semaphore = _locks.GetOrAdd(_name, _ => new SemaphoreSlim(1, 1));
            var acquired = await semaphore.WaitAsync(timeout ?? TimeSpan.FromSeconds(30), cancellationToken);
            if (!acquired) {
                throw new TimeoutException($"Failed to acquire lock '{_name}'");
            }
            return new InMemoryLockHandle(semaphore);
        }

        public IDistributedSynchronizationHandle? TryAcquire(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
            var semaphore = _locks.GetOrAdd(_name, _ => new SemaphoreSlim(1, 1));
            var acquired = semaphore.Wait(timeout, cancellationToken);
            return acquired ? new InMemoryLockHandle(semaphore) : null;
        }

        public async ValueTask<IDistributedSynchronizationHandle?> TryAcquireAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
            var semaphore = _locks.GetOrAdd(_name, _ => new SemaphoreSlim(1, 1));
            var acquired = await semaphore.WaitAsync(timeout, cancellationToken);
            return acquired ? new InMemoryLockHandle(semaphore) : null;
        }
    }

    internal sealed class InMemoryLockHandle : IDistributedSynchronizationHandle {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public InMemoryLockHandle(SemaphoreSlim semaphore) {
            _semaphore = semaphore;
        }

        public CancellationToken HandleLostToken => CancellationToken.None;

        public void Dispose() {
            if (!_disposed) {
                _semaphore.Release();
                _disposed = true;
            }
        }

        public ValueTask DisposeAsync() {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
