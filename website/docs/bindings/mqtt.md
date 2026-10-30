---
sidebar_position: 9
title: MQTT bindings
description: Read and write MQTT server, operation, and message bindings.
---

# MQTT

`BindingsCollection.MQTT` registers server, operation, and message parsers (not a channel parser). This page covers the MQTT binding types in `ByteBard.AsyncAPI.Bindings.MQTT`; MQTT 5's distinct registration is documented [separately](mqtt5.md).

## Write

The union setters accept their scalar/schema alternatives implicitly. A `long` becomes `MQTTInteger`; an `AsyncApiJsonSchema` becomes `MQTTIntegerSchema` or `MQTTStringSchema`. `AsyncApiJsonSchemaReference` also works through inheritance from `AsyncApiJsonSchema`—there is no separate reference conversion operator.

```csharp
using ByteBard.AsyncAPI.Bindings.MQTT;
using ByteBard.AsyncAPI.Models;

var server = new AsyncApiServer();
server.Bindings.Add(new MQTTServerBinding
{
    ClientId = "worker-1",
    KeepAlive = 60,
    SessionExpiryInterval = 500L,
    MaximumPacketSize = new AsyncApiJsonSchema { Type = SchemaType.Integer },
});

var operation = new AsyncApiOperation();
operation.Bindings.Add(new MQTTOperationBinding
{
    MessageExpiryInterval = new AsyncApiJsonSchemaReference("#/components/schemas/expiry"),
});

var message = new AsyncApiMessage();
var scalarResponse = new MQTTMessageBinding { ResponseTopic = "replies" };
var schemaResponse = new MQTTMessageBinding
{
    ResponseTopic = new AsyncApiJsonSchema { Type = SchemaType.String },
};
var referenceResponse = new MQTTMessageBinding
{
    ResponseTopic = new AsyncApiJsonSchemaReference("#/components/schemas/replyTopic"),
};
var scalarMessage = new AsyncApiMessage();
scalarMessage.Bindings.Add(scalarResponse);
var schemaMessage = new AsyncApiMessage();
schemaMessage.Bindings.Add(schemaResponse);
message.Bindings.Add(referenceResponse);
```

`Bindings.Add(binding)` stores the binding using its `BindingKey` (`mqtt`); callers do not need to supply the key.

## Read and use

Register MQTT parsers and pattern-match the runtime alternative before reading a union value. A schema reference is returned inside the schema alternative; inspect `Schema` to distinguish a reference from an inline schema.

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.MQTT;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.MQTT };
var reader = new AsyncApiStringReader(settings);
var scalarServer = reader.ReadFragment<AsyncApiServer>("""
    host: broker.example
    protocol: mqtt
    bindings:
      mqtt:
        sessionExpiryInterval: 500
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (scalarServer.Bindings.TryGetValue<MQTTServerBinding>(out var mqtt) &&
    mqtt.SessionExpiryInterval is MQTTInteger interval)
{
    Console.WriteLine(interval.Value);
}

var schemaServer = reader.ReadFragment<AsyncApiServer>("""
    host: broker.example
    protocol: mqtt
    bindings:
      mqtt:
        maximumPacketSize:
          $ref: '#/components/schemas/packetSize'
    """, AsyncApiVersion.AsyncApi3_0, out _);
if (schemaServer.Bindings.TryGetValue<MQTTServerBinding>(out var schemaBinding) &&
    schemaBinding.MaximumPacketSize is MQTTIntegerSchema { Schema: AsyncApiJsonSchemaReference reference })
{
    Console.WriteLine(reference.Reference.Reference);
}

var scalarMessage = reader.ReadFragment<AsyncApiMessage>("""
    bindings:
      mqtt:
        responseTopic: replies
    """, AsyncApiVersion.AsyncApi3_0, out _);
if (scalarMessage.Bindings.TryGetValue<MQTTMessageBinding>(out var messageBinding) &&
    messageBinding.ResponseTopic is MQTTString responseTopic)
{
    Console.WriteLine(responseTopic.Value);
}

var schemaMessage = reader.ReadFragment<AsyncApiMessage>("""
    bindings:
      mqtt:
        responseTopic:
          $ref: '#/components/schemas/replyTopic'
    """, AsyncApiVersion.AsyncApi3_0, out _);
if (schemaMessage.Bindings.TryGetValue<MQTTMessageBinding>(out var schemaMessageBinding) &&
    schemaMessageBinding.ResponseTopic is MQTTStringSchema { Schema: AsyncApiJsonSchemaReference responseReference })
{
    Console.WriteLine(responseReference.Reference.Reference);
}
```

`TryGetValue<TBinding>` looks up the binding using `TBinding.BindingKey` and returns `false` when that binding is absent. The union pattern checks remain necessary because the binding property may hold a scalar or schema alternative.

## Object model

MQTT field limits and topic syntax are not validated.

### `MQTTServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `ClientId` | `clientId` | `string` | Client identifier. |
| `CleanSession` | `cleanSession` | `bool?` | Whether the connection is non-persistent. Called clean start in MQTT 5. |
| `LastWill` | `lastWill` | `LastWill` | Last Will and Testament configuration. |
| `KeepAlive` | `keepAlive` | `int?` | Longest time, in seconds, the broker and client can go without sending a message. |
| `SessionExpiryInterval` | `sessionExpiryInterval` | `MQTTIntegerOrSchema` | Seconds the broker keeps the session of a disconnected client. |
| `MaximumPacketSize` | `maximumPacketSize` | `MQTTIntegerOrSchema` | Largest packet, in bytes, the client accepts. |

### `MQTTOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `MessageExpiryInterval` | `messageExpiryInterval` | `MQTTIntegerOrSchema` | Lifetime of the message, in seconds. |

### `MQTTMessageBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `PayloadFormatIndicator` | `payloadFormatIndicator` | `int?` | `0` for unspecified bytes, `1` for UTF-8 text. |
| `CorrelationData` | `correlationData` | `AsyncApiJsonSchema` | Identifies the request a response is for. |
| `ContentType` | `contentType` | `string` | Content type of the payload. |
| `ResponseTopic` | `responseTopic` | `MQTTStringOrSchema` | Topic for the response message. |

### `LastWill`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Topic` | `topic` | `string` | Topic the Last Will message is sent to. |
| `QoS` | `qos` | `uint?` | Delivery guarantee: `0`, `1` or `2`. |
| `Message` | `message` | `string` | The Last Will message. |

### Value unions

A union property holds one of two alternatives. Pattern-match it before reading the value. Both are written directly, without a wrapper object, and a schema is written for the AsyncAPI version of the document.

| Union | Alternative | Holds | Read from |
| --- | --- | --- | --- |
| `MQTTIntegerOrSchema` | `MQTTInteger` | `Value` (`long`) | an integer |
| | `MQTTIntegerSchema` | `Schema` (`AsyncApiJsonSchema`) | a schema or a `$ref` |
| `MQTTStringOrSchema` | `MQTTString` | `Value` (`string`) | a string |
| | `MQTTStringSchema` | `Schema` (`AsyncApiJsonSchema`) | a schema or a `$ref` |

A `long`, `string` or `AsyncApiJsonSchema` converts implicitly to the matching union. A non-integer where an integer is expected fails to read.
