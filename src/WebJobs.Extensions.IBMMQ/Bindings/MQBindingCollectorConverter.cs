using Azure.WebJobs.Extensions.IBMMQ.Config;
using Microsoft.Azure.WebJobs;

namespace Azure.WebJobs.Extensions.IBMMQ.Bindings;

internal class MQBindingCollectorConverter : IConverter<MQQueueAttribute, IAsyncCollector<MQMessage>>
{
    private readonly MQExtensionConfigProvider _configProvider;

    public MQBindingCollectorConverter(MQExtensionConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public IAsyncCollector<MQMessage> Convert(MQQueueAttribute attribute)
    {
        return new MQBindingCollector(_configProvider.CreateContext(attribute, "MQQueue-Output"));
    }
}