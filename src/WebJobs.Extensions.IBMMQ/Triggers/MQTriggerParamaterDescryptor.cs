using System.Text.Json;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Azure.WebJobs.Extensions.IBMMQ.Triggers;

internal class MQTriggerParamaterDescryptor : TriggerParameterDescriptor
{
    public string QueueName { get; }

    public MQTriggerParamaterDescryptor(string queueName)
    {
        QueueName = queueName;
    }

    public override string GetTriggerReason(IDictionary<string, string> arguments)
    {
        return $"IBMMQ message detected from queue: {QueueName} at {DateTime.Now} " +
               $"with Arguments {JsonSerializer.Serialize(arguments)}";
    }
}