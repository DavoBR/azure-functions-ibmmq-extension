# IBMMQ Extension for Azure Functions

This extension provides functionality for receiving IBM MQ messages in Azure Functions, allowing you to easily write
functions that respond to any message published to IBM MQ.

## Available bindings

| Name           | Direction | Description                                      |
| -------------- | --------- | ------------------------------------------------ |
| MQQueueTrigger | in        | Execute function when a new queue message arrive |
| MQQueueInput   | in        | Get a message from queue                         |
| MQQueueOutput  | out       | Put a message to queue                           |

## Getting started

- set up localhost.settings.json
  When running IBM MQ and Azurite locally, copy `local.settings.localhost.json` to `local.settings.json`.  
  When running IBM MQ and Azurite in devcontainers, copy `local.settings.devcontainer.json` to `local.settings.json`.
- Run the WebJobs sample with: `cd samples/WebJobs.Extensions.Samples && func start --verbose`

## Running examples

If you don't have an MQ server available you can quickly have one using docker and running the following command.
You can then run the ```Worker.Extensions.Samples``` project.

```bash
# IBM MQ web console: <https://:9443/ibmmq/console>  (admin/passw0rd)
# https://www.ibm.com/docs/en/ibm-mq/9.3.x
docker run -it --rm -e LICENSE=accept -e MQ_DEV=true -e MQ_QMGR_NAME=QM1 -e TZ=Europe/Amsterdam -p 1414:1414 -p 9443:9443 -p 9157:9157 --name ibmmq icr.io/ibm-messaging/mq:latest
```
