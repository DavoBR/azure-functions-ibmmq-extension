using System.Reflection;
using Azure.WebJobs.Extensions.IBMMQ.Config;
using Azure.WebJobs.Extensions.IBMMQ.Clients;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Triggers;

internal class MQTriggerAttributeBindingProvider : ITriggerBindingProvider
{
    private readonly MQExtensionConfigProvider _configProvider;

    public MQTriggerAttributeBindingProvider(MQExtensionConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
    {
        var parameter = context.Parameter!;
        var attribute = parameter.GetCustomAttribute<MQQueueTriggerAttribute>(inherit: false);

        if (attribute is null) {
            return Task.FromResult<ITriggerBinding>(null!);
        }

        var triggerContext = _configProvider.CreateContext(attribute, parameter);
        var triggerBinding = new MQTriggerBinding(triggerContext);

        return Task.FromResult<ITriggerBinding>(triggerBinding);
    }
}