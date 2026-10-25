---
sidebar_position: 17
title: WebSockets bindings
description: Read and write WebSockets channel bindings.
---

# WebSockets

`BindingsCollection.Websockets` registers `WebSocketsChannelBinding` only. (The collection property's capitalization is `Websockets`.)

```csharp
using ByteBard.AsyncAPI.Bindings.WebSockets;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new WebSocketsChannelBinding
{
    Method = "GET",
    Query = new AsyncApiJsonSchema
    {
        Type = SchemaType.Object,
        Properties = { ["room"] = new AsyncApiJsonSchema { Type = SchemaType.String } },
    },
    Headers = new AsyncApiJsonSchemaReference("#/components/schemas/connectionHeaders"),
});
```

`Bindings.Add(binding)` uses the `WebSocketsChannelBinding`'s `BindingKey` (`ws`); the dictionary key is not supplied separately.

There are no server, operation, or message parsers registered for this binding. The properties are listed in the [object model](#object-model).

## Write

Attach the typed binding at channel level, then serialize the enclosing document:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings.WebSockets;
using ByteBard.AsyncAPI.Models;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Chat", Version = "1.0.0" },
    Channels = new Dictionary<string, AsyncApiChannel>
    {
        ["chat"] = new AsyncApiChannel { Address = "chat" },
    },
};
document.Channels["chat"].Bindings.Add(new WebSocketsChannelBinding
{
    Method = "GET",
    Query = new AsyncApiJsonSchema { Type = SchemaType.Object },
    Headers = new AsyncApiJsonSchema { Type = SchemaType.Object },
});

var yaml = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
```

## Read and use

```csharp
using System;
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.WebSockets;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Websockets };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      ws:
        method: GET
        query:
          type: object
          properties:
            room:
              type: string
        headers:
          $ref: '#/components/schemas/connectionHeaders'
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);
if (parsed.Bindings.TryGetValue<WebSocketsChannelBinding>(out var webSockets))
{
    Console.WriteLine(webSockets.Query.Properties["room"].Type);
    if (webSockets.Headers is AsyncApiJsonSchemaReference headersReference)
    {
        Console.WriteLine(headersReference.Reference.Reference);
    }
}
```

`TryGetValue<TBinding>` returns `false` when no binding with that type's `BindingKey` is present.

The collection property is spelled `Websockets`; the binding key is `ws`. These examples target AsyncAPI 3.1.0; writing 2.x also converts the surrounding document layout. The model does not perform an HTTP upgrade or validate headers/query definitions. See the [shared guide](index.md).

## Object model

### `WebSocketsChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Method` | `method` | `string` | HTTP method used to establish the connection: `GET` or `POST`. |
| `Query` | `query` | `AsyncApiJsonSchema` | Schema of the query parameters, as an inline schema or an `AsyncApiJsonSchemaReference`. |
| `Headers` | `headers` | `AsyncApiJsonSchema` | Schema of the HTTP headers used to establish the connection, as an inline schema or an `AsyncApiJsonSchemaReference`. |
