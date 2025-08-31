# IBMMQ Extension for Azure Functions

This extension provides functionality for receiving IBM MQ messages in Azure Functions, allowing you to easily write
functions that respond to any message published to IBM MQ.

## Available bindings

| Name           | Direction | Description                                      |
| -------------- | --------- | ------------------------------------------------ |
| MQQueueTrigger | in        | Execute function when a new queue message arrive |
| MQQueueInput   | in        | Get a message from queue                         |
| MQQueueOutput  | out       | Put a message to queue                           |

## Supported types

[MQExtensionConfigProvider](src\WebJobs.Extensions.IBMMQ\Config\MQExtensionConfigProvider.cs) facilitates various types to use with the bindings, you can use the following:

(1) Native message with MQ properties, without conversion (requires a NuGet reference to [IBMXMSDotnetClient](https://www.nuget.org/packages/IBMXMSDotnetClient)):

- `IBM.XMS.IMessage`

(2) Converted message with some popular MQ properties (see [MessageConverters - CreateMessage method](src\WebJobs.Extensions.IBMMQ\Config\MessageConverters.cs)):

- `Azure.WebJobs.Extensions.IBMMQ.MQMessage`
- `Azure.WebJobs.Extensions.IBMMQ.MQTextMessage`
- `Azure.WebJobs.Extensions.IBMMQ.MQBytesMessage`

(3) Converted message, only the body, without MQ properties:

- `byte[]`
- `string`

## Example connection strings

As value for `MQ_CONNECTION_STRING`.

Certificate authentication:

`Host=192.168.50.209;Port=1414;Channel=DEV.APP.SVRCONN;SSLCipherSpec=TLS_RSA_WITH_AES_256_CBC_SHA256;SSLCertLabel=mykey;SSLKeyRepo=*USER`

Password authentication:

`Host=localhost;Port=1414;Channel=DEV.APP.SVRCONN;UserId=app;Password=passw0rd`

## Running examples

If you don't have an MQ server available you can quickly have one using docker and running the following command:

```bash
docker run -e LICENSE=accept -e MQ_QMGR_NAME=QMGR -p 1414:1414 -p 9443:9443 -detach --name QMGR icr.io/ibm-messaging/mq:latest
```

To configure certificate authentication on the MQ-side, consider the following `.mqsc` script, which you can place into the container at `/etc/mqm/` and it will automatically execute:

```
-- Enable SSL/TLS on the SVRCONN channel
ALTER CHANNEL('DEV.APP.SVRCONN') CHLTYPE(SVRCONN) SSLCIPH('ANY_TLS12')

-- Optional: Require clients to present a valid certificate for authentication
ALTER CHANNEL('DEV.APP.SVRCONN') CHLTYPE(SVRCONN) SSLCAUTH(REQUIRED)

-- Optional but recommended: Disable CHLAUTH temporarily to allow SSL-based access
ALTER QMGR CHLAUTH(DISABLED)

-- Apply and refresh SSL security settings
REFRESH SECURITY TYPE(SSL)
```

Then you can run either:

1. the `Worker.Extensions.Samples` project. Please note that this uses the assembly from NuGet, read more about it [here](https://blog.maartenballiauw.be/post/2021/06/01/custom-bindings-with-azure-functions-dotnet-isolated-worker.html).
1. the `WebJobs.Extensions.Samples` project. This runs the binding directly from [/src/](./src/).