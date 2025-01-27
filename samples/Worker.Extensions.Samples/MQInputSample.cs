using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Worker.Extensions.Samples;

public static class MQInputSample
{
    [Function(nameof(MQInputSample))]
    public static void RunAsync(
        [MQQueueTrigger("%MQ_QUEUE_INPUT%", Connection = "MQ_CONNECTION_STRING")] byte[] input,
        string messageId,
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(MQInputSample));
        log.LogInformation("========== MQINPUT ========================");
        log.LogInformation("Message ID: {MessageId}", messageId);
        log.LogInformation("Message Body: {MessageBody}", Encoding.UTF8.GetString(input));
        log.LogInformation("========== MQINPUT ========================");
    }
}