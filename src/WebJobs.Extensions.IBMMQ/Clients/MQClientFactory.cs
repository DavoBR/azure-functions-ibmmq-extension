using System.Collections.Concurrent;

namespace Azure.WebJobs.Extensions.IBMMQ.Clients
{
    internal class MQClientFactory : IDisposable
    {
        private readonly ConcurrentDictionary<string, MQClient> _cache = new();
        private bool _disposed;

        public MQClient CreateClient(string connectionString)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MQClientFactory));

            return _cache.GetOrAdd(connectionString, _ => new MQClient(connectionString));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    foreach (var client in _cache.Values) {
                        client.Dispose();
                    }

                    _cache.Clear();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
