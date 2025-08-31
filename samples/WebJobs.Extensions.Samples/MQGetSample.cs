using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WebJobs.Extensions.Samples;

public static class MQGetSample
{

    [FunctionName(nameof(MQGetSample))]
    public static Task RunAsync(
        [TimerTrigger("* * * * *")] TimerInfo timerInfo,
        [MQQueue("%MQ_QUEUE_OUTPUT%", Connection = "MQ_CONNECTION_STRING")] string? input,
        ILogger log)
    {
        log.LogInformation("========== MQGET ========================");
        log.LogInformation("Message Body: {MessageBody}", input);
        log.LogInformation("========== MQGET ========================");

        return Task.CompletedTask;
    }
}