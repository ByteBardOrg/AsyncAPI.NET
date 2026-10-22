---
sidebar_position: 2
title: AMQP bindings
description: Read and write AMQP channel, operation, and message bindings.
---

# AMQP

AMQP parsers are registered by `BindingsCollection.AMQP` (also included by `All`). Supported levels are channel (`AMQPChannelBinding`), operation (`AMQPOperationBinding`), and message (`AMQPMessageBinding`).

## Write

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings.AMQP;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new AMQPChannelBinding
{
    Is = ChannelType.RoutingKey,
    Exchange = new Exchange
    {
        Name = "orders",
        Type = ExchangeType.Topic,
        Durable = true,
        Vhost = "/",
    },
});

var operation = new AsyncApiOperation();
operation.Bindings.Add(new AMQPOperationBinding
{
    Expiration = 60_000,
    UserId = "publisher",
    Cc = new List<string> { "orders.created" },
    Priority = 5,
    DeliveryMode = DeliveryMode.Persistent,
    Mandatory = true,
    Ack = true,
});
```

Set `Is` to `ChannelType.Queue` and provide `Queue` for a queue channel; set it to `ChannelType.RoutingKey` and provide `Exchange` for an exchange channel. Serialization writes only the matching `Queue` or `Exchange` object.

## Read and use

Register the AMQP parsers, parse a channel fragment, then retrieve the typed binding with `TryGetValue<TBinding>`:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.AMQP;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.AMQP };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      amqp:
        is: queue
        queue:
          name: orders
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<AMQPChannelBinding>(out var amqp))
{
    Console.WriteLine(amqp.Is == ChannelType.Queue
        ? amqp.Queue.Name
        : amqp.Exchange.Name);
}

var parsedOperation = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>("""
    bindings:
      amqp:
        expiration: 60000
        priority: 5
        deliveryMode: 2
        mandatory: true
        ack: true
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedOperation.Bindings.TryGetValue<AMQPOperationBinding>(out var amqpOperation))
{
    Console.WriteLine(amqpOperation.Expiration);
}
```

Channel fields: `Is` (`ChannelType`), `Exchange` (`Exchange`), and `Queue` (`Queue`). `Exchange` carries `Name`, `Type`, `Durable`, `AutoDelete`, and `Vhost`; `Queue` carries `Name`, `Durable`, `Exclusive`, `AutoDelete`, and `Vhost`. Both `Vhost` values default to `/`. The enum choices are defined by `ChannelType` and `ExchangeType`.

Operation fields include `Expiration` (`uint`), `UserId`, `Cc`/`Bcc` lists, `Priority`, `DeliveryMode`, `Mandatory`, `Timestamp`, and `Ack`. Message fields are `ContentEncoding` and `MessageType`.

At the other supported levels, attach `AMQPOperationBinding` to an operation and `AMQPMessageBinding` to a message. Numeric fields and flags are represented by CLR numeric/boolean types; the model does not enforce broker-side limits or semantic combinations.
