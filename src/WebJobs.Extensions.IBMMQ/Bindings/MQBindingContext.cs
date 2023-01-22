using Azure.WebJobs.Extensions.IBMMQ.Clients;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Bindings;

internal class MQBindingContext
{
    public string QueueName { get; set; } = null!;
    
    public string ConnectionString { get; set; } = null!;

    public MQClientFactory ClientFactory { get; set; } = null!;

    public ILogger Logger { get; set; } = null!;
}