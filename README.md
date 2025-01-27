# IBMMQ Extension for Azure Functions

This extension provides functionality for receiving IBM MQ messages in Azure Functions, allowing you to easily write
functions that respond to any message published to IBM MQ.

## Available bindings

| Name           | Direction | Description                                      |
| -------------- | --------- | ------------------------------------------------ |
| MQQueueTrigger | in        | Execute function when a new queue message arrive |
| MQQueueInput   | in        | Get a message from queue                         |
| MQQueueOutput  | out       | Put a message to queue                           |

## Running examples

If you don't have an MQ server available you can quickly have one using docker and running the following command, then
you can run the ```Worker.Extensions.Samples``` project

```bash
docker run -e LICENSE=accept -e MQ_QMGR_NAME=QMGR -p 1414:1414 -p 9443:9443 -detach --name QMGR icr.io/ibm-messaging/mq:latest
```
