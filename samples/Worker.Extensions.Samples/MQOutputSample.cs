using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Worker.Extensions.Samples;

public static class MQOutputSample
{
    [Function(nameof(MQOutputSample))]
    [MQQueueOutput("%MQ_QUEUE_INPUT%", Connection = "MQ_CONNECTION_STRING")]
    public static string RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo timerInfo, FunctionContext context)
    {
        var log = context.GetLogger(nameof(MQOutputSample));

        var output = $"Hello World {DateTime.Now}";

        log.LogInformation("========== MQOUTPUT ========================");
        log.LogInformation("Message Body: {MessageBody}", output);
        log.LogInformation("========== MQOUTPUT ========================");

        return output;
    }
}