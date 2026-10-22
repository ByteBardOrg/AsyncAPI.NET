---
sidebar_position: 8
title: Kafka bindings
description: Read and write Kafka server, channel, operation, and message bindings.
---

# Kafka

Kafka is registered at server, channel, operation, and message levels by `BindingsCollection.Kafka`.

## Write

Attach channel-level topic settings with the typed Kafka model:

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings.Kafka;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new KafkaChannelBinding
{
    Topic = "orders",
    Partitions = 6,
    Replicas = 3,
    TopicConfiguration = new TopicConfigurationObject
    {
        CleanupPolicy = new List<string> { "delete", "compact" },
        RetentionMilliseconds = 86_400_000,
        AdditionalProperties = new Dictionary<string, AsyncApiAny>
        {
            ["custom.integer"] = new AsyncApiAny(12),
            ["custom.enabled"] = new AsyncApiAny(true),
        },
    },
});

var operation = new AsyncApiOperation();
operation.Bindings.Add(new KafkaOperationBinding
{
    GroupId = new AsyncApiJsonSchemaReference("#/components/schemas/groupId"),
    ClientId = new AsyncApiJsonSchema { Type = SchemaType.String },
});

var message = new AsyncApiMessage();
message.Bindings.Add(new KafkaMessageBinding
{
    Key = new AsyncApiJsonSchemaReference("#/components/schemas/messageKey"),
});
```

## Read and use

Register the Kafka parsers, parse a channel fragment, and retrieve the typed channel binding and topic:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Kafka;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Kafka };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      kafka:
        topic: orders
        partitions: 6
        replicas: 3
        topicConfiguration:
          cleanup.policy:
            - delete
            - compact
          retention.ms: 86400000
          custom.integer: 12
          custom.enabled: true
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<KafkaChannelBinding>(out var kafka))
{
    Console.WriteLine(kafka.Topic);
    Console.WriteLine(kafka.TopicConfiguration.AdditionalProperties["custom.integer"].GetValue<decimal>());
}

var parsedOperation = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>("""
    bindings:
      kafka:
        groupId:
          $ref: '#/components/schemas/groupId'
        clientId:
          type: string
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedOperation.Bindings.TryGetValue<KafkaOperationBinding>(out var kafkaOperation)
    && kafkaOperation.GroupId is AsyncApiJsonSchemaReference groupIdReference)
{
    Console.WriteLine(groupIdReference.Reference.Reference);
}

var parsedMessage = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>("""
    bindings:
      kafka:
        key:
          $ref: '#/components/schemas/messageKey'
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedMessage.Bindings.TryGetValue<KafkaMessageBinding>(out var kafkaMessage)
    && kafkaMessage.Key is AsyncApiJsonSchemaReference keyReference)
{
    Console.WriteLine(keyReference.Reference.Reference);
}
```

Server fields: `SchemaRegistryUrl`, `SchemaRegistryVendor`. Channel fields: `Topic`, nullable `Partitions`/`Replicas`, and typed `TopicConfigurationObject`. Its known cleanup/retention/message-size settings have typed properties; unrecognized settings are retained as `AdditionalProperties`, a dictionary of `AsyncApiAny` values. Operation fields: schema-valued `GroupId` and `ClientId`. Message fields: schema-valued `Key`, `SchemaIdLocation`, `SchemaIdPayloadEncoding`, and `SchemaLookupStrategy`.

`GroupId`, `ClientId`, and message `Key` are `AsyncApiJsonSchema` values and accept either an inline schema or an `AsyncApiJsonSchemaReference` directly. The same collection supports `KafkaServerBinding`, `KafkaOperationBinding`, and `KafkaMessageBinding` on their matching model elements. Numeric settings are nullable where indicated; server-specific Kafka constraints are not checked here.
