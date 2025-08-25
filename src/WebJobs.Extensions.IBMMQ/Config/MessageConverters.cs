using IBM.XMS;

namespace Azure.WebJobs.Extensions.IBMMQ.Config;

internal static class MessageConverters
{
    public static string? MessageToString(IMessage? message)
    {
        return message switch {
            null => null,
            ITextMessage textMessage => textMessage.Text,
            _ => throw new InvalidOperationException(
                $"The message is type {message.GetType().Name} expected {nameof(ITextMessage)}")
        };
    }

    public static string? MessageToString(MQMessage message)
    {
        if (message is MQTextMessage textMessage) {
            return textMessage.Text;
        }

        throw new InvalidOperationException("Not Text Message");
    }

    public static byte[]? MessageToBytes(MQMessage message)
    {
        if (message is MQBytesMessage bytesMessage) {
            return bytesMessage.Bytes;
        }

        throw new InvalidOperationException("Not Bytes Message");
    }

    public static byte[]? MessageToBytes(IMessage? message)
    {
        switch (message) {
            case null:
                return null;
            case IBytesMessage bytesMessage: {
                    var bytes = new byte[bytesMessage.BodyLength];
                    bytesMessage.ReadBytes(bytes);
                    return bytes;
                }
            default:
                throw new InvalidOperationException(
                    $"The message is type {message.GetType().Name} expected {nameof(IBytesMessage)}");
        }
    }

    public static MQMessage? MessageToMessage(IMessage? message)
    {
        switch (message) {
            case null:
                return null;
            case ITextMessage textMessage: {
                    var mqMessage = CreateMessage<MQTextMessage>(textMessage);
                    mqMessage.Text = MessageToString(textMessage);
                    return mqMessage;
                }
            case IBytesMessage bytesMessage: {
                    var mqMessage = CreateMessage<MQBytesMessage>(bytesMessage);
                    mqMessage.Bytes = MessageToBytes(bytesMessage);
                    return mqMessage;
                }
            default:
                throw new InvalidOperationException($"Unknown message type {message.GetType().Name}");
        }
    }

    public static MQTextMessage? MessageToTextMessage(IMessage? message)
    {
        return MessageToMessage(message) as MQTextMessage;
    }

    public static MQBytesMessage? MessageToBytesMessage(IMessage message)
    {
        return MessageToMessage(message) as MQBytesMessage;
    }

    public static MQMessage BytesToMessage(byte[] bytes)
    {
        return new MQBytesMessage(bytes);
    }

    public static MQMessage StringToMessage(string text)
    {
        return new MQTextMessage(text);
    }

    private static TMessage CreateMessage<TMessage>(IMessage source) where TMessage : MQMessage
    {
        var message = Activator.CreateInstance<TMessage>();

        message.Format = source.GetStringProperty(XMSC.JMS_IBM_FORMAT);
        message.Encoding = source.GetIntProperty(XMSC.JMS_IBM_ENCODING);
        message.CharacterSet = source.GetIntProperty(XMSC.JMS_IBM_CHARACTER_SET);
        message.MessageId = source.JMSMessageID?.Replace("ID:", "");
        message.CorrelationId = source.JMSCorrelationID?.Replace("ID:", "");
        message.ReplyTo = source.JMSReplyTo?.Name;
        message.ApplicationId = source.GetStringProperty(XMSC.JMSX_APPID).Trim();
        message.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(source.JMSTimestamp).DateTime;
        message.DeliveryMode = source.JMSDeliveryMode;
        message.Priority = source.JMSPriority;
        message.Redelivered = source.JMSRedelivered;
        message.Expiration = message.Timestamp - DateTimeOffset.FromUnixTimeMilliseconds(source.JMSExpiration).DateTime;
        message.SourceQueue = source.JMSDestination?.Name;

        return message;
    }
}