---
sidebar_position: 4
title: Google Pub/Sub bindings
description: Read and write Google Pub/Sub channel and message bindings.
---

# Google Pub/Sub

`BindingsCollection.GooglePubSub` registers channel and message levels; it is also part of `All`.

## Write

Attach channel-level settings to an `AsyncApiChannel`:

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings.GooglePubSub;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new GooglePubSubChannelBinding
{
    Labels = new Dictionary<string, string> { ["team"] = "fulfillment" },
    MessageRetentionDuration = "86400s",
    MessageStoragePolicy = new MessageStoragePolicy
    {
        AllowedPersistenceRegions = new[] { "us-central1", "us-east1" },
    },
    SchemaSettings = new SchemaSettings
    {
        Encoding = "JSON",
        FirstRevisionId = "1",
        LastRevisionId = "3",
        Name = "projects/example/schemas/orders",
    },
});

var message = new AsyncApiMessage();
message.Bindings.Add(new GooglePubSubMessageBinding
{
    Attributes = new Dictionary<string, string> { ["source"] = "billing" },
    OrderingKey = "customer-7",
    Schema = new MessageSchema { Name = "projects/example/schemas/orders" },
});
```

## Read and use

Register the channel and message parsers, parse a channel fragment, and access a typed field from the channel's `Bindings` dictionary:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.GooglePubSub;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.GooglePubSub };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      googlepubsub:
        labels:
          team: fulfillment
        messageRetentionDuration: 86400s
        messageStoragePolicy:
          allowedPersistenceRegions:
            - us-central1
            - us-east1
        schemaSettings:
          encoding: JSON
          firstRevisionId: '1'
          lastRevisionId: '3'
          name: projects/example/schemas/orders
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<GooglePubSubChannelBinding>(out var pubSub))
{
    Console.WriteLine(pubSub.Labels["team"]);
    foreach (var region in pubSub.MessageStoragePolicy.AllowedPersistenceRegions)
    {
        Console.WriteLine(region);
    }
    Console.WriteLine(pubSub.SchemaSettings.Name);
}

var parsedMessage = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>("""
    bindings:
      googlepubsub:
        schema:
          name: projects/example/schemas/orders
        orderingKey: customer-7
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedMessage.Bindings.TryGetValue<GooglePubSubMessageBinding>(out var pubSubMessage))
{
    Console.WriteLine(pubSubMessage.Schema.Name);
}
```

Channel fields: `Labels` (`Dictionary<string,string>`), `MessageRetentionDuration` (`string`), `MessageStoragePolicy` (`AllowedPersistenceRegions`, a sequence of strings), and `SchemaSettings` (`Encoding`, `FirstRevisionId`, `LastRevisionId`, and `Name`, all strings). Message fields: `Attributes` (`Dictionary<string,string>`), `OrderingKey`, and `Schema` (`MessageSchema`, whose `Name` is a string). This message `Schema` is Pub/Sub schema metadata, not an `AsyncApiJsonSchema` payload schema.

For message-level writes, attach a `GooglePubSubMessageBinding` with `message.Bindings.Add(new GooglePubSubMessageBinding { ... });`; it provides `Attributes`, `OrderingKey`, and `Schema`. Duration text and nested schema/storage settings are retained as model data; the library does not verify them against a live topic.
