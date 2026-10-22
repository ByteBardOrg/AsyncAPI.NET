---
sidebar_position: 15
title: Solace bindings
description: Read and write Solace server and operation bindings.
---

# Solace

`BindingsCollection.Solace` registers server and operation levels.

```csharp
using ByteBard.AsyncAPI.Bindings.Solace;
using ByteBard.AsyncAPI.Models;

var operation = new AsyncApiOperation();
operation.Bindings.Add(new SolaceOperationBinding
{
    TimeToLive = 60,
    Priority = new AsyncApiJsonSchemaReference("#/components/schemas/priority"),
});
```

`Bindings.Add(binding)` stores the binding under its `BindingKey` (`solace`).

Server fields are `MsgVpn` and `ClientName`. Operation fields include `Destinations` (`List<SolaceDestination>`), `TimeToLive` and `Priority` (`SolaceIntegerOrSchema`), and nullable `DmqEligible`. A destination has binding version, destination type, delivery mode, and either queue or topic detail; queue/topic classes expose topic subscriptions and other destination settings.

`SolaceIntegerOrSchema` is an abstract base type with two concrete alternatives: `SolaceInteger` exposes the numeric `Value`, while `SolaceIntegerSchema` exposes its `Schema` (which can itself be an `AsyncApiJsonSchemaReference`). Construct either alternative explicitly when setting a field. The current API also provides implicit conversions from `long` and `AsyncApiJsonSchema`; the explicit constructors make the selected alternative clear.

## Write

Attach the typed operation binding at operation level and serialize the enclosing document:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings.Solace;
using ByteBard.AsyncAPI.Models;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Orders", Version = "1.0.0" },
    Components = new AsyncApiComponents
    {
        Schemas = new Dictionary<string, AsyncApiMultiFormatSchema>
        {
            ["priority"] = new AsyncApiMultiFormatSchema
            {
                Schema = new AsyncApiJsonSchema { Type = SchemaType.Integer },
            },
        },
    },
    Channels = new Dictionary<string, AsyncApiChannel>
    {
        ["orders"] = new AsyncApiChannel { Address = "orders" },
    },
    Operations = new Dictionary<string, AsyncApiOperation>
    {
        ["publishOrders"] = new AsyncApiOperation
        {
            Action = AsyncApiAction.Send,
            Channel = new AsyncApiChannelReference("#/channels/orders"),
        },
    },
};
document.Operations["publishOrders"].Bindings.Add(new SolaceOperationBinding
{
    TimeToLive = 60,
    Priority = new AsyncApiJsonSchemaReference("#/components/schemas/priority"),
});

var yaml = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
```

## Read and use

Continuing from the Write example above, this snippet uses its `yaml` variable.

```csharp
using System;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Solace;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Solace };
var parsed = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
if (parsed.Operations["publishOrders"].Bindings.TryGetValue<SolaceOperationBinding>(out var solace))
{
    if (solace.TimeToLive is SolaceInteger ttl)
    {
        Console.WriteLine($"TTL seconds: {ttl.Value}");
    }

    if (solace.Priority is SolaceIntegerSchema priority &&
        priority.Schema is AsyncApiJsonSchemaReference priorityReference)
    {
        Console.WriteLine($"Priority schema ref: {priorityReference.Reference.Reference}");
    }
}
```

`TryGetValue<TBinding>` looks up the binding under the supplied type's `BindingKey` and returns `false` if it is absent. The property matches then distinguish the integer and schema/reference union alternatives.

The same collection supports `SolaceServerBinding` on a server. These examples target AsyncAPI 3.1.0; AsyncAPI 2.x output also transforms the document layout. The package does not verify VPN or destination configuration. See the [shared guide](index.md).
