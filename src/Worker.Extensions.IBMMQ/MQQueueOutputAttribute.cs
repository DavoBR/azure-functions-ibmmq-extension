using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Functions.Worker;

public sealed class MQQueueOutputAttribute(string queueName) : OutputBindingAttribute
{

    /// <summary>
    /// El nombre de la cola 
    /// </summary>
    public string QueueName { get; } = queueName;

    /// <summary>
    /// Parametros de conexión del Queue Manager. Formato: HostName=bhasdt1.cfbhd.com;Port=1414;Channel=CHL.MS
    /// </summary>
    public string Connection { get; set; } = null!;
}