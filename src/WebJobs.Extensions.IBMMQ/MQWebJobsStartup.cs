using Azure.WebJobs.Extensions.IBMMQ;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;

[assembly: WebJobsStartup(typeof(MQWebJobsStartup))]

namespace Azure.WebJobs.Extensions.IBMMQ;

public class MQWebJobsStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder) {
        builder.AddIBMMQ();
    }
}