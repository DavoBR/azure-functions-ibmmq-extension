using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Functions.Worker;

public sealed class MQQueueTriggerAttribute : TriggerBindingAttribute
{
    public MQQueueTriggerAttribute(string queueName) {
        QueueName = queueName;
    }
    
    /// <summary>
    /// El nombre de la cola 
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Parametros de conexión del Queue Manager. Formato: HostName=localhost;Port=1414;Channel=CHL.MS
    /// </summary>
    public string Connection { get; set; } = null!;
}