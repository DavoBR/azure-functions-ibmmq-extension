using Microsoft.Azure.WebJobs.Description;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.WebJobs;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Binding]
public class MQQueueAttribute : Attribute
{
    public MQQueueAttribute(string queueName) {
        QueueName = queueName;
    }
    
    /// <summary>
    /// El nombre de la cola 
    /// </summary>
    [AutoResolve]
    public string QueueName { get; }

    /// <summary>
    /// El MessageId
    /// </summary>
    [AutoResolve]
    public string? MessageId { get; }

    /// <summary>
    /// El CorrelationId
    /// </summary>
    [AutoResolve]
    public string? CorrelationId { get; }

    /// <summary>
    /// Parametros de conexión del Queue Manager. Formato: HostName=bhasdt1.cfbhd.com;Port=1414;Channel=CHL.MS
    /// </summary>
    [ConnectionString]
    public string Connection { get; set; } = null!;
}