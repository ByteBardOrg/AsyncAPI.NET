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

## Object model

The model does not enforce broker-side limits or combinations of values.

### `AMQPChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Is` | `is` | `ChannelType` | Whether the channel is a queue or a routing key. |
| `Exchange` | `exchange` | `Exchange` | The exchange, when `Is` is `RoutingKey`. |
| `Queue` | `queue` | `Queue` | The queue, when `Is` is `Queue`. |

### `AMQPOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Expiration` | `expiration` | `uint` | Time-to-live of the message. |
| `UserId` | `userId` | `string` | The user who sent the message. |
| `Cc` | `cc` | `List<string>` | Routing keys the message is routed to when published. |
| `Bcc` | `bcc` | `List<string>` | Like `Cc`, but consumers do not receive it. |
| `Priority` | `priority` | `int` | Priority of the message. |
| `DeliveryMode` | `deliveryMode` | `DeliveryMode` | Transient or persistent delivery. |
| `Mandatory` | `mandatory` | `bool` | Whether the message is mandatory. |
| `Timestamp` | `timestamp` | `bool` | Whether the message includes a timestamp. |
| `Ack` | `ack` | `bool` | Whether the consumer should acknowledge the message. |

### `AMQPMessageBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `ContentEncoding` | `contentEncoding` | `string` | MIME encoding of the message content. |
| `MessageType` | `messageType` | `string` | Application-specific message type. |

### `Exchange`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Name` | `name` | `string` | Name of the exchange, at most 255 characters. |
| `Type` | `type` | `ExchangeType` | Type of the exchange. |
| `Durable` | `durable` | `bool` | Whether the exchange survives broker restarts. |
| `AutoDelete` | `autoDelete` | `bool` | Whether the exchange is deleted when its last queue is unbound. |
| `Vhost` | `vhost` | `string` | Virtual host of the exchange. Defaults to `/`. |

### `Queue`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Name` | `name` | `string` | Name of the queue, at most 255 characters. |
| `Durable` | `durable` | `bool` | Whether the queue survives broker restarts. |
| `Exclusive` | `exclusive` | `bool` | Whether only one connection can use the queue. |
| `AutoDelete` | `autoDelete` | `bool` | Whether the queue is deleted when its last consumer unsubscribes. |
| `Vhost` | `vhost` | `string` | Virtual host of the queue. Defaults to `/`. |

### Enums

| Enum | Values (YAML) |
| --- | --- |
| `ChannelType` | `Queue` (`queue`), `RoutingKey` (`routingKey`) |
| `ExchangeType` | `Default` (`default`), `Topic` (`topic`), `Direct` (`direct`), `Fanout` (`fanout`), `Headers` (`headers`) |
| `DeliveryMode` | `Transient` (`1`), `Persistent` (`2`). Written as a number. |
