using Azure.WebJobs.Extensions.IBMMQ.Listeners;
using IBM.XMS;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Azure.WebJobs.Extensions.IBMMQ.Triggers;

internal class MQTriggerBinding : ITriggerBinding
{
    private readonly MQTriggerContext _context;
    
    public MQTriggerBinding(MQTriggerContext context) 
    {
        _context = context;

        BindingDataContract = CreateBindingDataContract();
    }

    public Type TriggerValueType => typeof(IMessage);
    
    public IReadOnlyDictionary<string, Type> BindingDataContract { get; }

    public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
    {
        var message = (IMessage)value;
        var valueProvider = new MQTriggerValueProvider(message, _context.ParameterInfo.ParameterType);
        var bindingData = CreateBindingData(message);
        
        return Task.FromResult<ITriggerData>(new TriggerData(valueProvider, bindingData));
    }

    public Task<IListener> CreateListenerAsync(ListenerFactoryContext context) {
        return Task.FromResult<IListener>(new MQListener(context.Descriptor.Id, context.Executor, _context));
    }

    public ParameterDescriptor ToParameterDescriptor() {
        return new MQTriggerParamaterDescryptor(_context.QueueName){
            Name = _context.ParameterInfo.Name,
            Type = "MQQueueTrigger"
        }; 
    }
    
    private static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
    {
        return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["MessageId"] = typeof(string),
            ["CorrelationId"] = typeof(string),
            ["ReplyTo"] = typeof(string),
            ["Redelivered"] = typeof(bool),
            ["EnqueuedTime"] = typeof(DateTime),
            ["EnqueuedTimeUtc"] = typeof(DateTime)
        };
    }

    private static IReadOnlyDictionary<string, object> CreateBindingData(IMessage message)
    {
        var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        SafeAddValue(() => bindingData.Add("MessageId", message.JMSMessageID.Replace("ID:", "")));
        SafeAddValue(() => bindingData.Add("CorrelationId", message.JMSCorrelationID.Replace("ID:", "")));
        SafeAddValue(() => bindingData.Add("ReplyTo", message.JMSReplyTo.Name));
        SafeAddValue(() => bindingData.Add("Redelivered", message.JMSRedelivered));
        SafeAddValue(() => {
            var enqueuedTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(message.JMSTimestamp);
            bindingData.Add("EnqueuedTimeUtc", enqueuedTimeUtc);
            bindingData.Add("EnqueuedTime", enqueuedTimeUtc.ToLocalTime());
        });

        return bindingData;
    }

    private static void SafeAddValue(Action addValue)
    {
        try
        {
            addValue();
        } catch (Exception) {
            // some message property getters can throw, based on the
            // state of the message
        }
    }
}