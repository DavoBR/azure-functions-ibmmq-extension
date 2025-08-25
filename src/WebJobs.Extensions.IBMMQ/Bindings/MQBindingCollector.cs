using Azure.WebJobs.Extensions.IBMMQ.Clients;
using IBM.XMS;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Bindings;

internal class MQBindingCollector : IAsyncCollector<MQMessage>
{
    private readonly MQBindingContext _context;
    private readonly ILogger _logger;
    private readonly string _details;
    private readonly Lazy<MQClient> _client;

    public MQBindingCollector(MQBindingContext context)
    {
        _context = context;
        _logger = context.Logger;
        _details = $"connection='{context.ConnectionString}', queue='{context.QueueName}'";
        _client = new Lazy<MQClient>(CreateClient);
    }

    private MQClient CreateClient()
    {
        return _context.ClientFactory.CreateClient(_context.ConnectionString);
    }

    public Task AddAsync(MQMessage message, CancellationToken cancellationToken)
    {
        var session = _client.Value.CreateSession();
        var destination = session.CreateQueue(_context.QueueName);

        if (message.DeliveryMode.HasValue) {
            destination.SetIntProperty(XMSC.WMQ_PERSISTENCE, (int)message.DeliveryMode);
        }

        if (message.Expiration.HasValue) {
            destination.SetIntProperty(XMSC.WMQ_EXPIRY, (int)message.Expiration.Value.TotalMilliseconds);
        }

        if (message.Priority.HasValue) {
            destination.SetIntProperty(XMSC.WMQ_PRIORITY, message.Priority.Value);
        }

        var producer = session.CreateProducer(destination);

        IMessage xmsMessage;

        switch (message) {
            case MQTextMessage textMessage:
                xmsMessage = session.CreateTextMessage(textMessage.Text);
                break;
            case MQBytesMessage bytesMessage: {
                    var xmsBytesMessage = session.CreateBytesMessage();
                    xmsBytesMessage.WriteBytes(bytesMessage.Bytes);
                    xmsMessage = xmsBytesMessage;
                    break;
                }
            default:
                throw new InvalidOperationException($"Unable to create Message from type {message.GetType().Name}");
        }

        SetMessageProperties(message, xmsMessage);

        if (message.ReplyTo is not null) {
            xmsMessage.JMSReplyTo = session.CreateQueue(message.ReplyTo);
        }

        try {
            producer.Send(xmsMessage);
        } catch (Exception ex) {
            _logger.LogError(ex, "Can't send message to queue ({Details})", _details);
            throw;
        }

        producer.Dispose();
        destination.Dispose();
        session.Dispose();

        return Task.CompletedTask;
    }

    private static void SetMessageProperties(MQMessage source, IMessage target)
    {
        if (source.Encoding is not null) {
            target.SetIntProperty(XMSC.JMS_IBM_ENCODING, source.Encoding.Value);
        }

        if (source.CharacterSet is not null) {
            target.SetIntProperty(XMSC.JMS_IBM_CHARACTER_SET, source.CharacterSet.Value);
        }

        if (source.Format is not null) {
            target.SetStringProperty(XMSC.JMS_IBM_FORMAT, source.Format);
        }

        if (source.MessageId is not null) {
            target.JMSMessageID = $"ID:{source.MessageId}";
        }

        if (source.CorrelationId is not null) {
            target.JMSCorrelationID = $"ID:{source.CorrelationId}";
        }

    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        // Batching not supported.
        return Task.FromResult(0);
    }
}