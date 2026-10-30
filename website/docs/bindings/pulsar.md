---
sidebar_position: 12
title: Pulsar bindings
description: Read and write Pulsar server and channel bindings.
---

# Pulsar

`BindingsCollection.Pulsar` registers server and channel levels.

```csharp
using ByteBard.AsyncAPI.Bindings.Pulsar;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Models.Bindings.Pulsar;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new PulsarChannelBinding
{
    Namespace = "sales",
    Persistence = Persistence.Persistent,
    Deduplication = true,
    Retention = new RetentionDefinition { Time = 7, Size = 1000 },
});

var server = new AsyncApiServer();
server.Bindings.Add(new PulsarServerBinding { Tenant = "contoso" });
```

`Bindings.Add(binding)` uses each instance's `BindingKey` (`pulsar`) for both channel and server bindings. The properties are listed in the [object model](#object-model).

## Write

For example, attach a `PulsarChannelBinding` to its channel and serialize the enclosing document:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings.Pulsar;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Models.Bindings.Pulsar;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Orders", Version = "1.0.0" },
    Channels = new Dictionary<string, AsyncApiChannel>
    {
        ["orders"] = new AsyncApiChannel { Address = "orders" },
    },
};
document.Channels["orders"].Bindings.Add(new PulsarChannelBinding
{
    Namespace = "sales",
    Persistence = Persistence.Persistent,
    Retention = new RetentionDefinition { Time = 7, Size = 1000 },
    Deduplication = true,
});

var yaml = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
```

## Read and use

Continuing from the Write example above, this snippet uses its `yaml` variable.

```csharp
using System;
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Pulsar;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Pulsar };
var parsed = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
if (parsed.Channels["orders"].Bindings.TryGetValue<PulsarChannelBinding>(out var pulsar))
{
    Console.WriteLine($"{pulsar.Namespace}: {pulsar.Persistence}, retention={pulsar.Retention.Time}m/{pulsar.Retention.Size}MB");
}

var parsedServer = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>("""
    bindings:
      pulsar:
        tenant: contoso
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedServer.Bindings.TryGetValue<PulsarServerBinding>(out var pulsarServer))
{
    Console.WriteLine(pulsarServer.Tenant);
}
```

Each typed lookup returns `false` when the corresponding server or channel binding is absent.

The same collection supports `PulsarServerBinding` on a server. These examples target AsyncAPI 3.1.0; when writing 2.x, AsyncAPI.NET converts the surrounding document layout. The model does not enforce Pulsar's deployment-specific retention or namespace constraints. For reader setup see the [shared guide](index.md).

## Object model

### `PulsarServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Tenant` | `tenant` | `string` | Pulsar tenant. `public` is assumed when omitted. |

### `PulsarChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Namespace` | `namespace` | `string` | Namespace of the topic. |
| `Persistence` | `persistence` | `Persistence` | Persistence of the topic. Always written; defaults to `Persistent`. |
| `Compaction` | `compaction` | `int?` | Topic compaction threshold, in bytes. |
| `GeoReplication` | `geo-replication` | `IEnumerable<string>` | Clusters the topic is replicated to. |
| `Retention` | `retention` | `RetentionDefinition` | Retention policy of the topic. |
| `TTL` | `ttl` | `int?` | Message time-to-live, in seconds. |
| `Deduplication` | `deduplication` | `bool?` | Whether each message is persisted only once. |

### `RetentionDefinition`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Time` | `time` | `int` | Retention time, in minutes. `0` disables retention. |
| `Size` | `size` | `int` | Retention size, in megabytes. `0` disables retention. |

### `Persistence`

| Value | YAML |
| --- | --- |
| `Persistent` | `persistent` |
| `NonPersistent` | `non-persistent` |
