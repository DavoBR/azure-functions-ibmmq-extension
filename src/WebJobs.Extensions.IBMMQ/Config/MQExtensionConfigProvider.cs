using System.Reflection;
using Azure.WebJobs.Extensions.IBMMQ.Bindings;
using Azure.WebJobs.Extensions.IBMMQ.Clients;
using Azure.WebJobs.Extensions.IBMMQ.Triggers;
using IBM.XMS;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Config;

[Extension("IBMMQ")]
// ReSharper disable once ClassNeverInstantiated.Global
internal class MQExtensionConfigProvider : IExtensionConfigProvider
{
    private readonly INameResolver _nameResolver;
    private readonly IConfiguration _config;
    private readonly ILoggerFactory _loggerFactory;
    private readonly MQClientFactory _clientFactory;

    public MQExtensionConfigProvider(
        INameResolver nameResolver,
        IConfiguration config,
        ILoggerFactory loggerFactory,
        MQClientFactory clientFactory)
    {
        _nameResolver = nameResolver;
        _config = config;
        _loggerFactory = loggerFactory;
        _clientFactory = clientFactory;
    }

    public void Initialize(ExtensionConfigContext context)
    {
        context
           // IMessage -> string
           .AddConverter<IMessage, string?>(MessageConverters.MessageToString)
           // IMessage -> byte[]
           .AddConverter<IMessage, byte[]?>(MessageConverters.MessageToBytes)
           // IMessage -> MQMessage
           .AddConverter<IMessage, MQMessage?>(MessageConverters.MessageToMessage)
           // IMessage -> MQTextMessage
           .AddConverter<IMessage, MQTextMessage?>(MessageConverters.MessageToTextMessage)
           // IBytesMessage -> MQBytesMessage
           .AddConverter<IMessage, MQBytesMessage?>(MessageConverters.MessageToBytesMessage)
           // byte[] <-> MQBaseMessage
           .AddConverter<byte[], MQMessage>(MessageConverters.BytesToMessage)
           .AddConverter<MQMessage, byte[]?>(MessageConverters.MessageToBytes)
           // string <-> MQBaseMessage
           .AddConverter<string, MQMessage>(MessageConverters.StringToMessage)
           .AddConverter<MQMessage, string?>(MessageConverters.MessageToString);

        // register trigger binding provider
        var triggerBindingRule = context.AddBindingRule<MQQueueTriggerAttribute>();
        triggerBindingRule.AddValidator((attr, _)
            => ValidateConnection(attr.Connection, $"{nameof(MQQueueTriggerAttribute)}.{nameof(MQQueueTriggerAttribute.Connection)}"));

        triggerBindingRule.BindToTrigger(new MQTriggerAttributeBindingProvider(this));

        // register binding provider
        var bindingRule = context.AddBindingRule<MQQueueAttribute>();
        bindingRule.AddValidator((attr, _)
            => ValidateConnection(attr.Connection, $"{nameof(MQQueueAttribute)}.{nameof(MQQueueAttribute.Connection)}"));

        bindingRule.BindToInput(new MQBindingConverter(this));
        bindingRule.BindToCollector(new MQBindingCollectorConverter(this));
    }

    private static void ValidateConnection(string connection, string attrProperty)
    {
        if (string.IsNullOrEmpty(connection)) {
            throw new InvalidOperationException(
                $"The MQ connection string must be set via the {attrProperty} property");
        }
    }

    public MQBindingContext CreateContext(MQQueueAttribute attribute, string logCategory)
    {
        var queueName = _nameResolver.ResolveWholeString(attribute.QueueName);
        var connectionString = ResolveConnectionString(attribute.Connection);
        return new MQBindingContext {
            QueueName = queueName,
            ConnectionString = connectionString,
            ClientFactory = _clientFactory,
            Logger = _loggerFactory.CreateLogger(logCategory)
        };
    }

    public MQTriggerContext CreateContext(MQQueueTriggerAttribute attribute, ParameterInfo parameterInfo)
    {
        var queueName = _nameResolver.ResolveWholeString(attribute.QueueName);
        var connectionString = ResolveConnectionString(attribute.Connection);

        return new MQTriggerContext {
            QueueName = queueName,
            ConnectionString = connectionString,
            Logger = _loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("MQQueueTrigger")),
            ParameterInfo = parameterInfo,
            ClientFactory = _clientFactory
        };
    }

    private string ResolveConnectionString(string connection)
    {
        var connectionString = _config.GetConnectionStringOrSetting(connection) ?? connection;

        var list = connectionString
            .Split(';')
            .Select(pair => pair.Split('='))
            .Where(kv => kv.Length == 2)
            .ToDictionary(kv => kv[0], kv => _nameResolver.ResolveWholeString(kv[1]))
            .Select(kv => $"{kv.Key}={kv.Value}");

        return string.Join(";", list);
    }
}