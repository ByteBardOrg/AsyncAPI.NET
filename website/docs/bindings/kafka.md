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

## Object model

Schema-valued properties (`AsyncApiJsonSchema`) accept an inline schema or an `AsyncApiJsonSchemaReference`. Kafka's own constraints on these values are not validated.

### `KafkaServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `SchemaRegistryUrl` | `schemaRegistryUrl` | `string` | API URL of the Schema Registry used when producing messages. |
| `SchemaRegistryVendor` | `schemaRegistryVendor` | `string` | Schema Registry and serdes vendor, such as `apicurio`, `confluent`, `ibm` or `karapace`. |

### `KafkaChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Topic` | `topic` | `string` | Topic name, if different from the channel name. |
| `Partitions` | `partitions` | `int?` | Number of partitions on the topic. |
| `Replicas` | `replicas` | `int?` | Number of replicas of the topic. |
| `TopicConfiguration` | `topicConfiguration` | `TopicConfigurationObject` | Topic configuration relevant to the API. See below. |

### `TopicConfigurationObject`

| Property | YAML field | Type |
| --- | --- | --- |
| `CleanupPolicy` | `cleanup.policy` | `List<string>` |
| `RetentionMilliseconds` | `retention.ms` | `long?` |
| `RetentionBytes` | `retention.bytes` | `long?` |
| `DeleteRetentionMilliseconds` | `delete.retention.ms` | `long?` |
| `MaxMessageBytes` | `max.message.bytes` | `int?` |
| `ConfluentKeySchemaValidation` | `confluent.key.schema.validation` | `bool?` |
| `ConfluentKeySubjectName` | `confluent.key.subject.name.strategy` | `string` |
| `ConfluentValueSchemaValidation` | `confluent.value.schema.validation` | `bool?` |
| `ConfluentValueSubjectName` | `confluent.value.subject.name.strategy` | `string` |
| `AdditionalProperties` | any other setting | `Dictionary<string, AsyncApiAny>` |

Settings without a typed property are read into `AdditionalProperties` and written back unchanged.

### `KafkaOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `GroupId` | `groupId` | `AsyncApiJsonSchema` | Id of the consumer group. |
| `ClientId` | `clientId` | `AsyncApiJsonSchema` | Id of the consumer inside the consumer group. |

### `KafkaMessageBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Key` | `key` | `AsyncApiJsonSchema` | The message key. |
| `SchemaIdLocation` | `schemaIdLocation` | `string` | Where a Schema Registry schema id is stored, such as `header` or `payload`. |
| `SchemaIdPayloadEncoding` | `schemaIdPayloadEncoding` | `string` | How the schema id is encoded in the payload, such as `confluent`, `apicurio-legacy` or `apicurio-new`. |
| `SchemaLookupStrategy` | `schemaLookupStrategy` | `string` | Naming strategy class for schema lookup. |

Every binding also has `BindingVersion` (`bindingVersion`).
