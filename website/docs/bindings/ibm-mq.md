---
sidebar_position: 6
title: IBM MQ bindings
description: Read and write IBM MQ server, channel, and message bindings.
---

# IBM MQ

`BindingsCollection.IBMMQ` registers server, channel, and message parsers. The binding namespace is `ByteBard.AsyncAPI.Bindings.IBMMQ`.

## Write

Server settings are attached to an `AsyncApiServer` under the `ibmmq` key:

```csharp
using ByteBard.AsyncAPI.Bindings.IBMMQ;
using ByteBard.AsyncAPI.Models;

var server = new AsyncApiServer();
server.Bindings.Add(new IBMMQServerBinding
{
    GroupId = "payments",
    HeartBeatInterval = 30,
});

var channel = new AsyncApiChannel();
channel.Bindings.Add(new IBMMQChannelBinding
{
    DestinationType = "queue",
    Queue = new Queue { ObjectName = "ORDERS", IsPartitioned = false, Exclusive = false },
    MaxMsgLength = 1_048_576,
});

var topicChannel = new AsyncApiChannel();
topicChannel.Bindings.Add(new IBMMQChannelBinding
{
    DestinationType = "topic",
    Topic = new Topic { StringValue = "orders/#", ObjectName = "ORDERS.TOPIC", DurablePermitted = false, LastMsgRetained = false },
});

var message = new AsyncApiMessage();
message.Bindings.Add(new IBMMQMessageBinding { Type = "binary", Headers = "STRING" });
```

## Read and use

Register IBM MQ parsers, parse a server fragment, and read the typed heartbeat interval:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.IBMMQ;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.IBMMQ };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>("""
    bindings:
      ibmmq:
        heartBeatInterval: 30
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<IBMMQServerBinding>(out var ibmMq))
{
    Console.WriteLine(ibmMq.HeartBeatInterval);
}

var parsedChannel = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      ibmmq:
        destinationType: queue
        queue:
          objectName: ORDERS
          isPartitioned: false
          exclusive: false
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedChannel.Bindings.TryGetValue<IBMMQChannelBinding>(out var ibmMqChannel))
{
    Console.WriteLine(ibmMqChannel.Queue.IsPartitioned);
}

var parsedMessage = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>("""
    bindings:
      ibmmq:
        headers: STRING
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedMessage.Bindings.TryGetValue<IBMMQMessageBinding>(out var ibmMqMessage))
{
    Console.WriteLine(ibmMqMessage.Headers);
}
```

## Object model

Nullable properties are omitted when unset, and an explicit `false` is kept. The model does not connect to IBM MQ or validate queue manager settings.

### `IBMMQServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `GroupId` | `groupId` | `string` | Group of related servers sharing this binding. |
| `CcdtQueueManagerName` | `ccdtQueueManagerName` | `string` | Queue manager name in the client channel definition table. |
| `CipherSpec` | `cipherSpec` | `string` | TLS cipher specification. |
| `MultiEndpointServer` | `multiEndpointServer` | `bool?` | Whether this server is one of several endpoints of the same service. |
| `HeartBeatInterval` | `heartBeatInterval` | `int?` | Heartbeat interval, in seconds. |

### `IBMMQChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `DestinationType` | `destinationType` | `string` | `topic` or `queue`. |
| `Queue` | `queue` | `Queue` | Queue settings, for a queue destination. |
| `Topic` | `topic` | `Topic` | Topic settings, for a topic destination. |
| `MaxMsgLength` | `maxMsgLength` | `int?` | Maximum message length, in bytes. |

### `IBMMQMessageBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Type` | `type` | `string` | Message type, such as `string`, `jms` or `binary`. |
| `Headers` | `headers` | `string` | MQRFH2 header names. A string, not a schema. |
| `Description` | `description` | `string` | Description of the message. |
| `Expiry` | `expiry` | `long?` | Message lifetime. |

### `Queue`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `ObjectName` | `objectName` | `string` | Name of the queue. |
| `IsPartitioned` | `isPartitioned` | `bool?` | Whether the queue is a cluster queue. |
| `Exclusive` | `exclusive` | `bool?` | Whether only one application can read from the queue. |

### `Topic`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `StringValue` | `string` | `string` | Topic string. |
| `ObjectName` | `objectName` | `string` | Name of the topic object. |
| `DurablePermitted` | `durablePermitted` | `bool?` | Whether durable subscriptions are allowed. |
| `LastMsgRetained` | `lastMsgRetained` | `bool?` | Whether the last message is retained. |
