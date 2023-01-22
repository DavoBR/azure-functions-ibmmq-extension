using Azure.WebJobs.Extensions.IBMMQ;
using Azure.WebJobs.Extensions.IBMMQ.Clients;
using Azure.WebJobs.Extensions.IBMMQ.Config;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for IBMMQ integration.
/// </summary>
public static class MQWebJobsBuilderExtensions
{
    /// <summary>
    /// Adds the IBMQMQ extension to the provided <see cref="IWebJobsBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
    // ReSharper disable once InconsistentNaming
    public static IWebJobsBuilder AddIBMMQ(this IWebJobsBuilder builder) {
        builder.AddExtension<MQExtensionConfigProvider>();
        
        builder.Services.TryAddSingleton<MQClientFactory>();
        
        return builder;
    }
}