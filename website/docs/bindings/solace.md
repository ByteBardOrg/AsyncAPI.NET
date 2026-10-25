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

The properties, including the `SolaceIntegerOrSchema` union, are listed in the [object model](#object-model).

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

## Object model

### `SolaceServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `MsgVpn` | `msgVpn` | `string` | Message VPN of the server. |
| `ClientName` | `clientName` | `string` | Name of the client. |

### `SolaceOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Destinations` | `destinations` | `List<SolaceDestination>` | Destinations of the operation. |
| `TimeToLive` | `timeToLive` | `SolaceIntegerOrSchema` | Message time-to-live, in milliseconds. |
| `Priority` | `priority` | `SolaceIntegerOrSchema` | Message priority. |
| `DmqEligible` | `dmqEligible` | `bool?` | Whether an expired message may go to the dead message queue. |

### `SolaceDestination`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `DestinationType` | `destinationType` | `string` | `queue` or `topic`. |
| `DeliveryMode` | `deliveryMode` | `string` | `direct` or `persistent`. |
| `Queue` | `queue` | `SolaceQueue` | Queue settings, for a queue destination. |
| `Topic` | `topic` | `SolaceTopic` | Topic settings, for a topic destination. |
| `BindingVersion` | `bindingVersion` | `string` | Binding version of the destination. |

### `SolaceQueue`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Name` | `name` | `string` | Name of the queue. |
| `TopicSubscriptions` | `topicSubscriptions` | `List<string>` | Topics the queue subscribes to. |
| `AccessType` | `accessType` | `string` | `exclusive` or `nonexclusive`. |
| `MaxMsgSpoolSize` | `maxMsgSpoolSize` | `string` | Maximum spool size of the queue. |
| `MaxTtl` | `maxTtl` | `string` | Maximum time-to-live of messages on the queue. |

### `SolaceTopic`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `TopicSubscriptions` | `topicSubscriptions` | `List<string>` | Topics to subscribe to. |

### `SolaceIntegerOrSchema`

A union of two alternatives. Pattern-match it before reading the value; both are written directly, without a wrapper object. A `long` or `AsyncApiJsonSchema` converts implicitly to the union, but constructing the alternative makes the choice explicit.

| Alternative | Holds | Read from |
| --- | --- | --- |
| `SolaceInteger` | `Value` (`long`) | an integer |
| `SolaceIntegerSchema` | `Schema` (`AsyncApiJsonSchema`) | a schema or a `$ref` |
