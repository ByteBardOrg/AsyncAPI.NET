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

Server fields include `GroupId`, `CcdtQueueManagerName`, `CipherSpec`, nullable `MultiEndpointServer`, and nullable `HeartBeatInterval`. Channel fields are `DestinationType`, `Queue`, `Topic`, and nullable `MaxMsgLength`; set `Queue` for queue settings (`ObjectName`, nullable `IsPartitioned` and `Exclusive`) or `Topic` for topic settings (`StringValue`, `ObjectName`, nullable `DurablePermitted` and `LastMsgRetained`). These nullable boolean values preserve explicit `false` and are omitted when unset. Message `Headers` is a string, not a schema; other fields are `Type`, `Description`, and nullable `Expiry` (`long?`).

The collection also supports channel (`IBMMQChannelBinding`) and message (`IBMMQMessageBinding`) bindings; attach them to the corresponding model element's `Bindings` dictionary. Binding values do not establish an IBM MQ connection or validate queue manager settings.
