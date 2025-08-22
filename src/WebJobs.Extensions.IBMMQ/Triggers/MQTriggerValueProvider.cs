using Azure.WebJobs.Extensions.IBMMQ.Config;
using IBM.XMS;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Azure.WebJobs.Extensions.IBMMQ.Triggers;

internal class MQTriggerValueProvider : IValueProvider
{
    private readonly IMessage _input;
    private readonly Lazy<string> _convertError;

    public MQTriggerValueProvider(IMessage input, Type destinationType)
    {
        _input = input;
        _convertError = new Lazy<string>(() =>
            $"Unable to convert {_input.GetType().Name} to '{Type}'. Check function method signature");
        Type = destinationType;
    }
    
    public Type Type { get; }
    
    public Task<object?> GetValueAsync()
    {
        object? obj;
        
        if (Type.IsInstanceOfType(_input)) {
            obj = _input;
        } else if (typeof(MQMessage).IsAssignableFrom(Type)) {
            obj = MessageConverters.MessageToMessage(_input);
        } else if (Type == typeof(byte[]) && _input is IBytesMessage bytesMessage) {
            obj = MessageConverters.MessageToBytes(bytesMessage);
        } else if (Type == typeof(string) && _input is ITextMessage textMessage) {
            obj = MessageConverters.MessageToString(textMessage);
        }  else {
            throw new InvalidOperationException(_convertError.Value);
        }

        return Task.FromResult(obj);
    }

    public string ToInvokeString()
    {
        return _input switch {
            ITextMessage textMessage => textMessage.Text,
            _ => $"[Input type {_input.GetType().Name} expected ITextMessage or String]"
        };
    }
    
    
}