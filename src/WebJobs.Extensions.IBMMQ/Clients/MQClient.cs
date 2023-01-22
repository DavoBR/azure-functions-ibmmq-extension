using Azure.WebJobs.Extensions.IBMMQ.Config;
using IBM.XMS;

namespace Azure.WebJobs.Extensions.IBMMQ.Clients;

internal class MQClient : IDisposable
{
    private readonly IDictionary<string, string> _parameters;
    private readonly IConnection _connection;
    private readonly List<ISession> _sessions = new();
    
    public event ExceptionListener? ExceptionListener;

    public MQClient(string connectionString)
    {
        _parameters = ConnectionStringHelper.Parse(connectionString);

        // Create XMS Factory
        var xmsFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
        // Create WMQ Connection Factory
        var cf = xmsFactory.CreateConnectionFactory();
        // Set properties
        SetConnectionProperties(cf, out var userId, out var password);
        // Create connection
        _connection = cf.CreateConnection(userId, password);
        _connection.ExceptionListener += ex => ExceptionListener?.Invoke(ex);
    }

    public ISession CreateSession()
    {
        _parameters.TryGetValue("AckMode", out var ackMode);
        
        // https://www.ibm.com/docs/en/ibm-mq/8.0?topic=sessions-message-acknowledgement#xms_cmesack
        var session = _connection.CreateSession(false, ackMode?.ToUpper() switch {
            "AUTO" => AcknowledgeMode.AutoAcknowledge,
            "CLIENT" => AcknowledgeMode.ClientAcknowledge,
            "DUPSOK" => AcknowledgeMode.DupsOkAcknowledge,
            _ => AcknowledgeMode.AutoAcknowledge
        });
        
        _sessions.Add(session);

        return session;
    }

    public void Start() => _connection.Start();

    public void Stop() => _connection.Stop();

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        
        _sessions.ForEach(s => s.Dispose());
        _connection.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void SetConnectionProperties(IConnectionFactory cf, out string? userId, out string? password) 
    {
        cf.SetIntProperty(XMSC.WMQ_CLIENT_RECONNECT_OPTIONS, XMSC.WMQ_CLIENT_RECONNECT);

        if (_parameters.TryGetValue("Host", out var host) && !string.IsNullOrEmpty(host)) {
            cf.SetStringProperty(XMSC.WMQ_HOST_NAME, host);
        } else {
            throw new InvalidOperationException("El parametro Host no esta configurado en el connection string");
        }

        if (_parameters.TryGetValue("Port", out var portText)) {
            if (int.TryParse(portText, out var port)) {
                cf.SetIntProperty(XMSC.WMQ_PORT, port);
            } else {
                throw new InvalidOperationException("El parametro Port no es un valor valido");
            }
        }

        if (_parameters.TryGetValue("Channel", out var channel) && !string.IsNullOrEmpty(channel)) {
            cf.SetStringProperty(XMSC.WMQ_CHANNEL, channel);
        } else {
            throw new InvalidOperationException("El parametro Channel no esta configurado en el connection string");
        }

        if (_parameters.TryGetValue("QueueManager", out var qmgr)) {
            cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, qmgr);
        }

        if (_parameters.TryGetValue("AppName", out var appName)) {
            cf.SetStringProperty(XMSC.WMQ_APPLICATIONNAME, appName);
        }

        _parameters.TryGetValue("UserId", out userId);

        _parameters.TryGetValue("Password", out password);
    }
}
