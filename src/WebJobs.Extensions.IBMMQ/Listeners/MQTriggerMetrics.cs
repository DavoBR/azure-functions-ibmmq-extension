using Microsoft.Azure.WebJobs.Host.Scale;

// ReSharper disable once CheckNamespace
namespace Azure.WebJobs.Extensions.IBMMQ;

internal class MQTriggerMetrics : ScaleMetrics
{
    /// <summary>
    /// The total number of unprocessed messages in queue.
    /// </summary>
    public long MessageCount { get; set; }
}