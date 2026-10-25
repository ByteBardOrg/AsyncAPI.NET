---
sidebar_position: 11
title: NATS bindings
description: Read and write NATS operation bindings.
---

# NATS

`BindingsCollection.NATS` registers only `NATSOperationBinding`.

```csharp
using ByteBard.AsyncAPI.Bindings.NATS;
using ByteBard.AsyncAPI.Models;

var operation = new AsyncApiOperation();
operation.Bindings.Add(new NATSOperationBinding { Queue = "workers" });
```

`Bindings.Add(binding)` stores the instance under its `BindingKey` (`nats`); no key string is needed.

There are no NATS server, channel, or message parsers in this collection.

## Write

Attach the typed binding to the operation's bindings collection, then serialize the containing document. The operation references its channel in this AsyncAPI 3.1.0 example:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings.NATS;
using ByteBard.AsyncAPI.Models;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Orders", Version = "1.0.0" },
    Channels = new Dictionary<string, AsyncApiChannel>
    {
        ["orders"] = new AsyncApiChannel { Address = "orders" },
    },
    Operations = new Dictionary<string, AsyncApiOperation>
    {
        ["consumeOrders"] = new AsyncApiOperation
        {
            Action = AsyncApiAction.Receive,
            Channel = new AsyncApiChannelReference("#/channels/orders"),
        },
    },
};
document.Operations["consumeOrders"].Bindings.Add(new NATSOperationBinding
{
    Queue = "workers",
});

var yaml = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
```

## Read and use

Continuing from the Write example above, this snippet uses its `yaml` variable.

```csharp
using System;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.NATS;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.NATS };
var parsed = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
if (parsed.Operations["consumeOrders"].Bindings.TryGetValue<NATSOperationBinding>(out var nats))
{
    Console.WriteLine(nats.Queue);
}
```

Typed lookup returns `false` if no NATS operation binding is present. These examples target AsyncAPI 3.1.0; if serializing as 2.x, the document's operation/channel layout is converted as well as the binding. Queue semantics are not checked against a running NATS server. See [shared setup](index.md).

## Object model

### `NATSOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Queue` | `queue` | `string` | Queue group name. |
