---
sidebar_position: 10
title: MQTT 5 bindings
description: Read and write MQTT 5 server bindings.
---

# MQTT 5

This binding type is marked obsolete in the source API and is retained for compatibility. It registers only a server binding under `mqtt5`; MQTT's current server/operation/message bindings are documented [separately](mqtt.md).

## Write

`SessionExpiryInterval` is `MQTTIntegerOrSchema`. A `long` and an `AsyncApiJsonSchema` convert implicitly to the union. `AsyncApiJsonSchemaReference` needs no separate operator: it derives from `AsyncApiJsonSchema` and uses the schema conversion. Select the value form you need, then attach the binding to a server:

```csharp
using ByteBard.AsyncAPI.Bindings.MQTT;
using ByteBard.AsyncAPI.Bindings.MQTT5;
using ByteBard.AsyncAPI.Models;

var scalarBinding = new MQTT5ServerBinding
{
    SessionExpiryInterval = 500,
};
var schemaBinding = new MQTT5ServerBinding
{
    SessionExpiryInterval = new AsyncApiJsonSchema { Type = SchemaType.Integer },
};
var referenceBinding = new MQTT5ServerBinding
{
    SessionExpiryInterval = new AsyncApiJsonSchemaReference("#/components/schemas/interval"),
};

var scalarServer = new AsyncApiServer();
scalarServer.Bindings.Add(scalarBinding);
var schemaServer = new AsyncApiServer();
schemaServer.Bindings.Add(schemaBinding);
var server = new AsyncApiServer();
server.Bindings.Add(referenceBinding);
```

`Bindings.Add(binding)` stores each instance under its `BindingKey` (`mqtt5`).

The integer, inline-schema, and reference alternatives are written directly, without a union wrapper object.

## Read and use

Register `BindingsCollection.MQTT5` to read this legacy binding. The scalar input produces `MQTTInteger`; the `$ref` input produces `MQTTIntegerSchema` whose `Schema` can be tested for `AsyncApiJsonSchemaReference`.

```csharp
using System;
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.MQTT;
using ByteBard.AsyncAPI.Bindings.MQTT5;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.MQTT5 };
var reader = new AsyncApiStringReader(settings);
var scalarServer = reader.ReadFragment<AsyncApiServer>("""
    host: broker.example
    protocol: mqtt
    bindings:
      mqtt5:
        sessionExpiryInterval: 500
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);
if (scalarServer.Bindings.TryGetValue<MQTT5ServerBinding>(out var scalar) &&
    scalar.SessionExpiryInterval is MQTTInteger integer)
{
    Console.WriteLine(integer.Value);
}

var referenceServer = reader.ReadFragment<AsyncApiServer>("""
    host: broker.example
    protocol: mqtt
    bindings:
      mqtt5:
        sessionExpiryInterval:
          $ref: '#/components/schemas/interval'
    """, AsyncApiVersion.AsyncApi3_0, out _);
if (referenceServer.Bindings.TryGetValue<MQTT5ServerBinding>(out var referenced) &&
    referenced.SessionExpiryInterval is MQTTIntegerSchema { Schema: AsyncApiJsonSchemaReference schemaReference })
{
    Console.WriteLine(schemaReference.Reference.Reference);
}
```

`TryGetValue<TBinding>` returns `false` if no binding with that type's `BindingKey` is present; the union pattern then distinguishes the scalar and schema values.

The union is also used by MQTT's server and operation fields. MQTT 5's binding is deprecated, and `BindingsCollection.MQTT5` has no channel, operation, or message parser. AsyncAPI 2.x and 3.x serialization are supported; specify a target version when serializing. See the [shared guide](index.md).

## Object model

### `MQTT5ServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `SessionExpiryInterval` | `sessionExpiryInterval` | `MQTTIntegerOrSchema` | Seconds the broker keeps the session of a disconnected client, as an integer or a schema. See [value unions](mqtt.md#value-unions). |
