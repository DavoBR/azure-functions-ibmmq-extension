using System.Collections.Concurrent;

namespace Azure.WebJobs.Extensions.IBMMQ.Clients;

internal class MQClientFactory : IDisposable
{
    private readonly ConcurrentDictionary<string, MQClient> _cache = new();

    public MQClient CreateClient(string connectionString)
    {
        return _cache.GetOrAdd(connectionString, _ => new MQClient(connectionString));
    }

    public void Dispose()
    {
        foreach (var client in _cache.Values) {
            client.Dispose();
        }
    }
}