using System.Collections;
using Azure.WebJobs.Extensions.IBMMQ.Config;
using IBM.WMQ;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;

// La logica de esta clase es la misma que se utiliza en el EventHubTrigger
// https://github.com/Azure/azure-functions-eventhubs-extension/blob/dev/src/Microsoft.Azure.WebJobs.Extensions.EventHubs/Listeners/EventHubsScaleMonitor.cs

namespace Azure.WebJobs.Extensions.IBMMQ.Listeners;

internal class MQScaleMonitor : IScaleMonitor<MQTriggerMetrics> {
    private readonly string _connectionString;
    private readonly string _queueName;
    private readonly ILogger _logger;

    public MQScaleMonitor(string functionId, string connectionString, string queueName, ILogger logger) {
        _connectionString = connectionString;
        _queueName = queueName;
        _logger = logger;

        Descriptor = new ScaleMonitorDescriptor($"{functionId}-MQQueueTrigger-{queueName}".ToLower(), functionId);
    }

    public ScaleMonitorDescriptor Descriptor { get; }

    async Task<ScaleMetrics> IScaleMonitor.GetMetricsAsync() {
        return await GetMetricsAsync();
    }

    private MQQueueManager CreateConnection() {
        var parameters = ConnectionStringHelper.Parse(_connectionString);

        parameters.TryGetValue("QueueManager", out var queueManagerName);
        parameters.TryGetValue("AppName", out var appName);

        var properties = new Hashtable {
            [MQC.HOST_NAME_PROPERTY] = parameters["Host"],
            [MQC.PORT_PROPERTY] = parameters["Port"],
            [MQC.CHANNEL_PROPERTY] = parameters["Channel"],
            [MQC.APPNAME_PROPERTY] = appName ?? Environment.MachineName,
            [MQC.TRANSPORT_PROPERTY] = MQC.TRANSPORT_MQSERIES_MANAGED
        };

        if (parameters.TryGetValue("SSLCipherSpec", out var sslCipherSpec)) {
            properties[MQC.SSL_CIPHER_SPEC_PROPERTY] = sslCipherSpec;
        }

        if (parameters.TryGetValue("SSLCertLabel", out var sslCertLabel)) {
            properties[MQC.CERT_LABEL_PROPERTY] = sslCertLabel;
        }

        if (parameters.TryGetValue("SSLCipherSuite", out var sslCipherSuite)) {
            properties[MQC.SSL_CIPHER_SUITE_PROPERTY] = sslCipherSuite;
        }

        if (parameters.TryGetValue("SSLPeerName", out var sslPeerName)) {
            properties[MQC.SSL_PEER_NAME_PROPERTY] = sslPeerName;
        }

        if (parameters.TryGetValue("SSLCertStore", out var sslCertStore)) {
            properties[MQC.SSL_CERT_STORE_PROPERTY] = sslCertStore;
        }

        if (parameters.TryGetValue("UserId", out var userId)) {
            properties[MQC.USER_ID_PROPERTY] = userId;
        }

        if (parameters.TryGetValue("Password", out var password)) {
            properties[MQC.PASSWORD_PROPERTY] = password;
        }

        return new MQQueueManager(queueManagerName, properties);
    }

    public Task<MQTriggerMetrics> GetMetricsAsync() {
        var metrics = new MQTriggerMetrics();

        _logger.LogInformation("Connecting to queue manager {ConnectionString}", _connectionString);

        using var qmgr = CreateConnection();

        _logger.LogInformation("Querying current depth for queue {Queue}", _queueName);

        var queue = qmgr.AccessQueue(_queueName, MQC.MQOO_INQUIRE);

        _logger.LogInformation("Queue current depth: {Count} messages", queue.CurrentDepth);

        metrics.MessageCount = queue.CurrentDepth;

        queue.Close();

        return Task.FromResult(metrics);
    }

    ScaleStatus IScaleMonitor.GetScaleStatus(ScaleStatusContext context) {
        return GetScaleStatusCore(context.WorkerCount, context.Metrics?.Cast<MQTriggerMetrics>().ToArray());
    }

    public ScaleStatus GetScaleStatus(ScaleStatusContext<MQTriggerMetrics> context) {
        return GetScaleStatusCore(context.WorkerCount, context.Metrics?.ToArray());
    }

    private ScaleStatus GetScaleStatusCore(int workerCount, IList<MQTriggerMetrics>? metrics) {
        var status = new ScaleStatus {
            Vote = ScaleVote.None
        };

        const int numberOfSamplesToConsider = 5;

        // Unable to determine the correct vote with no metrics.
        if (metrics == null || metrics.Count == 0) {
            return status;
        }

        // At least 5 samples are required to make a scale decision for the rest of the checks.
        if (metrics.Count < numberOfSamplesToConsider) {
            return status;
        }

        // Check to see if the Queue has been empty for a while. Only if all metrics samples are empty do we scale down.
        var isIdle = metrics.All(m => m.MessageCount == 0);
        if (isIdle) {
            status.Vote = ScaleVote.ScaleIn;
            _logger.LogInformation("'{QueueName}' is idle", _queueName);
            return status;
        }

        // Samples are in chronological order. Check for a continuous increase in unprocessed event count.
        // If detected, this results in an automatic scale out for the site container.
        if (metrics[0].MessageCount > 0) {
            var msgCountIncreasing =
                IsTrueForLastN(
                    metrics,
                    numberOfSamplesToConsider,
                    (prev, next) => prev.MessageCount < next.MessageCount);

            if (msgCountIncreasing) {
                status.Vote = ScaleVote.ScaleOut;
                _logger.LogInformation("Message count is increasing for '{QueueName}'", _queueName);
                return status;
            }
        }

        var msgCountDecreasing =
            IsTrueForLastN(
                metrics,
                numberOfSamplesToConsider,
                (prev, next) => prev.MessageCount > next.MessageCount);

        if (msgCountDecreasing) {
            status.Vote = ScaleVote.ScaleIn;
            _logger.LogInformation("Message count is decreasing for '{QueueName}'", _queueName);
            return status;
        }

        _logger.LogInformation("Queue '{QueueName}' is steady", _queueName);

        return status;
    }

    private static bool IsTrueForLastN(IList<MQTriggerMetrics> samples, int count, Func<MQTriggerMetrics, MQTriggerMetrics, bool> predicate) {
        // Walks through the list from left to right starting at len(samples) - count.
        for (var i = samples.Count - count; i < samples.Count - 1; i++) {
            if (!predicate(samples[i], samples[i + 1])) {
                return false;
            }
        }

        return true;
    }
}