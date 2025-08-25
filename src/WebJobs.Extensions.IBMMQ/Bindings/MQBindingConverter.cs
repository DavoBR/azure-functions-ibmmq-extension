using Azure.WebJobs.Extensions.IBMMQ.Config;
using Azure.WebJobs.Extensions.IBMMQ.Clients;
using IBM.XMS;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Azure.WebJobs.Extensions.IBMMQ.Bindings;

internal class MQBindingConverter : IConverter<MQQueueAttribute, IMessage>
{
    private readonly MQExtensionConfigProvider _configProvider;

    public MQBindingConverter(MQExtensionConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public IMessage Convert(MQQueueAttribute attribute)
    {
        var context = _configProvider.CreateContext(attribute, "MQQueue-Input");
        var logger = context.Logger;
        var clientFactory = context.ClientFactory;
        var details = $"connection='{context.ConnectionString}', queue='{context.QueueName}'";

        ISession? session = null;
        IDestination? destination = null;
        IMessageConsumer? consumer = null;

        try {
            var client = clientFactory.CreateClient(context.ConnectionString);

            session = client.CreateSession();
            destination = session.CreateQueue(context.QueueName);
            consumer = session.CreateConsumer(destination);

            client.Start();

            return consumer.ReceiveNoWait();
        } catch (Exception) {
            logger.LogError("Error receive message ({Details}", details);
            throw;
        } finally {
            consumer?.Dispose();
            destination?.Dispose();
            session?.Dispose();
        }
    }
}