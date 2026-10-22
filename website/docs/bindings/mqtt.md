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

Server fields include `ClientId`, nullable `CleanSession`, `LastWill`, nullable `KeepAlive`, `SessionExpiryInterval`, and `MaximumPacketSize`. Operation has `MessageExpiryInterval`. Message fields include nullable `PayloadFormatIndicator`, schema-valued `CorrelationData`, `ContentType`, and `ResponseTopic`.

`SessionExpiryInterval`, `MaximumPacketSize`, and `MessageExpiryInterval` use `MQTTIntegerOrSchema`; `ResponseTopic` uses `MQTTStringOrSchema`. On read, scalar integers/strings become `MQTTInteger`/`MQTTString`; schema objects and `$ref` objects become `MQTTIntegerSchema`/`MQTTStringSchema`. Both alternatives serialize directly without an extra wrapper, and schema serialization follows the enclosing AsyncAPI version. Invalid integer scalars fail integer parsing. MQTT field limits and topic syntax are not validated.

The same collection supports `MQTTOperationBinding` and `MQTTMessageBinding` at operation and message level. These union fields may also contain schemas, so pattern-match the active alternative before using it; see the conversion notes above.
