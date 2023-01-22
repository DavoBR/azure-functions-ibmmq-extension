using System.Reflection;
using Azure.WebJobs.Extensions.IBMMQ.Clients;
using IBM.XMS;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Triggers;

// ReSharper disable once InconsistentNaming
internal class MQTriggerContext
{
    public string QueueName { get; set; } = null!;

    public string ConnectionString { get; set; } = null!;
    
    public ILogger Logger = null!;

    public ParameterInfo ParameterInfo = null!;

    public MQClientFactory ClientFactory { get; set; } = null!;
}
