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

        if (!parameters.ContainsKey("Host")) {
            throw new InvalidOperationException("The property 'Host' is required in the connection string");
        }

        if (!parameters.ContainsKey("Port")) {
            throw new InvalidOperationException("The property 'Port' is required in the connection string");
        }

        if (!parameters.ContainsKey("Channel")) {
            throw new InvalidOperationException("The property 'Channel' is required in the connection string");
        }

        return parameters;
    }
}