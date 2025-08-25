using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WebJobs.Extensions.Samples;

public static class MQOutputSample
{
    [FunctionName(nameof(MQOutputSample))]
    public static Task RunAsync(
        [TimerTrigger("* * * * *")] TimerInfo timerInfo,
        [MQQueue("%MQ_QUEUE_INPUT%", Connection = "MQ_CONNECTION_STRING")] out string output,
        ILogger log)
    {
        var input = $"Hello World {DateTime.Now}";

        log.LogInformation("========== MQOUTPUT ========================");
        log.LogInformation("Message Body: {MessageBody}", input);
        log.LogInformation("========== MQOUTPUT ========================");

        output = input;

        return Task.CompletedTask;
    }
}