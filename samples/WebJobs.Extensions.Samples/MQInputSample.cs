using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WebJobs.Extensions.Samples;

public static class MQInputSample
{
    [FunctionName(nameof(MQInputSample))]
    public static Task RunAsync(
        [MQQueueTrigger("%MQ_QUEUE_INPUT%", Connection = "MQ_CONNECTION_STRING")] string input,
        [MQQueue("%MQ_QUEUE_OUTPUT%", Connection = "MQ_CONNECTION_STRING")] out string output,
        string messageId,
        ILogger log)
    {
        log.LogInformation("========== MQINPUT ========================");
        log.LogInformation("Message ID: {MessageId}", messageId);
        log.LogInformation("Message Body: {MessageBody}", input);
        log.LogInformation("========== MQINPUT ========================");
        
        output = input;

        return Task.CompletedTask;
    }
}