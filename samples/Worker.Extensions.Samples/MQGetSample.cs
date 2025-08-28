using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Worker.Extensions.Samples;

public static class MQGetSample
{
    [Function(nameof(MQGetSample))]
    public static Task RunAsync(
        [TimerTrigger("*/5 * * * * *")] TimerInfo timerInfo,
        [MQQueueInput("%MQ_QUEUE_OUTPUT%", Connection = "MQ_CONNECTION_STRING")] string? input,
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(MQGetSample));
        log.LogInformation("========== MQGET ========================");
        log.LogInformation("Message Body: {MessageBody}", input);
        log.LogInformation("========== MQGET ========================");

        return Task.CompletedTask;
    }
}