using Azure.WebJobs.Extensions.IBMMQ.Clients;
using Azure.WebJobs.Extensions.IBMMQ.Triggers;
using IBM.WMQAX;
using IBM.XMS;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Listeners;

internal sealed class MQListener : IListener, IScaleMonitorProvider
{
    private readonly string _functionId;
    private readonly ITriggeredFunctionExecutor _executor;
    private readonly MQTriggerContext _context;
    private readonly ILogger _logger;
    private readonly Lazy<MQScaleMonitor> _scaleMonitor;
    private readonly Lazy<MQClient> _client;
    private readonly string _details;

    private bool _started;
    private ISession? _session;
    private IDestination? _destination;
    private IMessageConsumer? _consumer;

    public MQListener(string functionId, ITriggeredFunctionExecutor executor, MQTriggerContext triggerContext)
    {
        _functionId = functionId;
        _executor = executor;
        _context = triggerContext;
        _logger = triggerContext.Logger;
        _scaleMonitor = new Lazy<MQScaleMonitor>(CreateMonitor);
        _client = new Lazy<MQClient>(CreateClient);
        _details = $"function='{functionId}', connection='{triggerContext.ConnectionString}', queue='{triggerContext.QueueName}'";
    }

    private MQScaleMonitor CreateMonitor()
    {
        return new MQScaleMonitor(_functionId, _context.ConnectionString, _context.QueueName, _logger);
    }

    private MQClient CreateClient()
    {
        var client = _context.ClientFactory.CreateClient(_context.ConnectionString);

        client.ExceptionListener += ex => ProcessErrorAsync(ex).Wait();

        _session = client.CreateSession();
        _destination = _session.CreateQueue(_context.QueueName);
        _consumer = _session.CreateConsumer(_destination);
        _consumer.MessageListener += msg => ProcessMessageAsync(msg).Wait();

        return client;
    }

    public IScaleMonitor GetMonitor()
    {
        return _scaleMonitor.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try {
            _client.Value.Start();

            _started = true;

            _logger.LogInformation("MQ listener started ({Details})", _details);

        } catch (Exception ex) {
            var exceptionMessage = "Can't start MQ listener";
            _logger.LogError(ex, "{ExceptionMessage} ({Details})", exceptionMessage, _details);
            throw new InvalidOperationException(exceptionMessage, ex);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_started) {
            return Task.CompletedTask;
        }

        _client.Value.Stop();

        _logger.LogInformation("MQ listener stopped ({Details})", _details);

        _started = false;

        return Task.CompletedTask;
    }

    public void Cancel()
    {
        StopAsync(CancellationToken.None).Wait();
    }

    public void Dispose()
    {
        StopAsync(CancellationToken.None).Wait();

        _consumer?.Dispose();
        _destination?.Dispose();
        _session?.Dispose();

        if (_client.IsValueCreated) {
            _client.Value.Dispose();
        }
    }

    private async Task ProcessMessageAsync(IMessage message)
    {
        var input = new TriggeredFunctionData {
            TriggerValue = message
        };

        await _executor.TryExecuteAsync(input, CancellationToken.None);
    }

    private Task ProcessErrorAsync(Exception ex)
    {
        if (ex is MQException { ReasonCode: 2545 }) {
            _logger.LogWarning(ex, "The MQ connection reconnected successfully and all handles are reinstated");
        } else {
            _logger.LogError(ex, "MQConnection error");
        }

        return Task.CompletedTask;
    }
}