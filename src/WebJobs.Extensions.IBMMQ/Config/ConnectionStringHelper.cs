namespace Azure.WebJobs.Extensions.IBMMQ.Config;

internal static class ConnectionStringHelper
{
    public static IDictionary<string, string> Parse(string connectionString)
    {
        var parameters = connectionString
            .Split(';')
            .Select(pair => pair.Split('='))
            .Where(kv => kv.Length == 2)
            .ToDictionary(kv =>
                    kv[0].ToUpper(),
                kv => kv[1],
                StringComparer.OrdinalIgnoreCase);

        if (!parameters.TryGetValue("Host", out var hostName)) {
            throw new InvalidOperationException("The property Host is required in the connection string");
        }

        if (!parameters.TryGetValue("Port", out var port)) {
            throw new InvalidOperationException("The property Port is required in the connection string");
        }

        if (!parameters.TryGetValue("Channel", out var channelName)) {
            throw new InvalidOperationException("The property Channel is required in the connection string");
        }

        return parameters;
    }
}