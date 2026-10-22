---
sidebar_position: 1
title: Protocol bindings
description: Read and write protocol-specific AsyncAPI bindings with ByteBard.AsyncAPI.NET.
---

# Protocol bindings

Bindings preserve protocol-specific metadata on AsyncAPI servers, channels, operations, and messages. The model types are in the `ByteBard.AsyncAPI.Bindings` package; reader support is opt-in.

## Read bindings

Assign the parser set to `AsyncApiReaderSettings.Bindings` before creating the reader. Choose the matching collection to limit parsing, or `All` to enable every parser shipped by this package:

```csharp
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Readers;

var yaml = """
    asyncapi: 3.0.0
    info:
      title: Orders
      version: 1.0.0
    channels:
      orders:
        address: orders
        bindings:
          kafka:
            topic: orders
    """;
var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Kafka };
var document = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
```

`BindingsCollection.All` is the convenience alternative. Diagnostics remain important: enabling a parser does not imply that every malformed field is accepted. If a document contains a binding whose parser is not registered, the reader emits a diagnostic and drops that binding; it cannot be round-tripped from the resulting model. The collection is grouped by protocol, not by binding level.

To register only one built-in parser, provide an individual parser instance instead of a protocol collection. For example, this enables parsing Kafka channel bindings only:

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings.Kafka;
using ByteBard.AsyncAPI.Models.Interfaces;
using ByteBard.AsyncAPI.Readers;
using ByteBard.AsyncAPI.Readers.Interface;

var settings = new AsyncApiReaderSettings
{
    Bindings = new List<IBindingParser<IBinding>>
    {
        new KafkaChannelBinding(),
    },
};
var document = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
```

The `IBindingParser<out T>` interface is covariant, so the built-in channel binding parser can be added to the reader's `IBindingParser<IBinding>` collection. Add further parser instances as needed for the binding levels present in the input.

## Write bindings

Bindings are ordinary typed model objects attached to the relevant model element. Add a binding instance with `Bindings.Add`; its `BindingKey` supplies the protocol key. Retrieve a registered binding with `Bindings.TryGetValue<TBinding>(out var binding)`. Populate the model, then serialize the containing AsyncAPI document with the normal document writer for your chosen AsyncAPI version. For example, a Kafka channel binding can be constructed and attached like this:

```csharp
using ByteBard.AsyncAPI.Bindings.Kafka;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Models.Interfaces;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new KafkaChannelBinding
{
    Topic = "orders",
    Partitions = 6,
    Replicas = 3,
});
```

At server, channel, operation, and message levels, use the binding type documented for that level. The library serializes the binding with the enclosing document; it does not create a broker connection or validate service-side constraints. See the protocol pages for registered levels, property types, and conversion caveats.

## Choose a protocol

- [AMQP](amqp.md) · [Anypoint MQ](anypointmq.md) · [Google Pub/Sub](google-pubsub.md) · [HTTP](http.md)
- [IBM MQ](ibm-mq.md) · [JMS](jms.md) · [Kafka](kafka.md) · [MQTT](mqtt.md)
- [MQTT 5](mqtt5.md) · [NATS](nats.md) · [Pulsar](pulsar.md) · [ROS 2](ros2.md)
- [Amazon SNS](sns.md) · [Solace](solace.md) · [Amazon SQS](sqs.md) · [WebSockets](websockets.md)
