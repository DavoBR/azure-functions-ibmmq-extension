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

If you don't have an MQ server available you can quickly have one using docker and running the following command.
You can then run the ```Worker.Extensions.Samples``` project.

```bash
# IBM MQ web console: <https://:9443/ibmmq/console>  (admin/passw0rd)
# https://www.ibm.com/docs/en/ibm-mq/9.3.x
docker run -it --rm -e LICENSE=accept -e MQ_DEV=true -e MQ_QMGR_NAME=QMGR -e TZ=Europe/Amsterdam -p 1414:1414 -p 9443:9443 -p 9157:9157 --name ibmmq icr.io/ibm-messaging/mq:latest
```

## When using devcontainers

When using devcontainer with docker-compose, use the service name from the docker-compose file as your hostname.
E.g. do not use 'localhost', but e.g. 'ibmmq' if that is your service name.
