using System.Text.Json;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Azure.WebJobs.Extensions.IBMMQ.Triggers;

internal class MQTriggerParameterDescriptor : TriggerParameterDescriptor
{
    public string QueueName { get; }

    public MQTriggerParameterDescriptor(string queueName)
    {
        QueueName = queueName;
    }

    public override string GetTriggerReason(IDictionary<string, string> arguments)
    {
        return $"IBMMQ message detected on queue: {QueueName} " +
               $"with Arguments {JsonSerializer.Serialize(arguments)}";
    }
}