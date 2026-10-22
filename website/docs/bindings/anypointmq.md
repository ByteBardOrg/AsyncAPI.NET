---
sidebar_position: 3
title: Anypoint MQ bindings
description: Read and write Anypoint MQ channel and message bindings.
---

# Anypoint MQ

`BindingsCollection.AnypointMQ` registers channel and message parsers (and is included in `All`). There is no Anypoint MQ server or operation binding parser in this collection.

## Write

Attach typed channel and message binding instances with `Bindings.Add`; the binding key is supplied by each binding type:

```csharp
using ByteBard.AsyncAPI.Bindings.AnypointMQ;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new AnypointMQChannelBinding
{
    Destination = "orders",
    DestinationType = "exchange",
});

var message = new AsyncApiMessage();
message.Bindings.Add(new AnypointMQMessageBinding
{
    Headers = new AsyncApiJsonSchema
    {
        Type = SchemaType.Object,
        Properties =
        {
            ["traceId"] = new AsyncApiJsonSchema { Type = SchemaType.String },
        },
    },
});
```

## Read and use

Register the Anypoint MQ parsers, parse a channel fragment, and use its typed destination field:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.AnypointMQ;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.AnypointMQ };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      anypointmq:
        destination: orders
        destinationType: queue
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<AnypointMQChannelBinding>(out var anypoint))
{
    Console.WriteLine(anypoint.Destination);
}

var parsedMessage = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>("""
    bindings:
      anypointmq:
        headers:
          type: object
          properties:
            traceId:
              type: string
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedMessage.Bindings.TryGetValue<AnypointMQMessageBinding>(out var anypointMessage))
{
    Console.WriteLine(anypointMessage.Headers.Properties["traceId"].Type);
}
```

Channel fields are `Destination` and `DestinationType` (strings). Message-level metadata is `Headers` (`AsyncApiJsonSchema`); attach it with `message.Bindings.Add(new AnypointMQMessageBinding { Headers = headers });`.

`Headers` accepts an inline schema or a schema reference, for example `new AsyncApiJsonSchemaReference("#/components/schemas/messageHeaders")`; after reading, inspect the resulting `Headers` schema or reference on the typed message binding.

The same registration includes message bindings; attach `AnypointMQMessageBinding` to a message when writing. Schemas are represented as `AsyncApiJsonSchema`; this package models metadata and does not contact Anypoint MQ or validate destination names.
