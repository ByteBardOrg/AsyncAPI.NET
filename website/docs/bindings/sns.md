---
sidebar_position: 14
title: Amazon SNS bindings
description: Read and write SNS channel and operation bindings.
---

# Amazon SNS

`BindingsCollection.Sns` registers channel and operation parsers.

```csharp
using ByteBard.AsyncAPI.Bindings.Sns;
using ByteBard.AsyncAPI.Models;

var operation = new AsyncApiOperation();
operation.Bindings.Add(new SnsOperationBinding
{
    Topic = new Identifier { Arn = "arn:aws:sns:us-east-1:123456789012:orders" },
});
```

Use `Bindings.Add(binding)` to let the binding's `BindingKey` (`sns`) determine its dictionary key.

The properties are listed in the [object model](#object-model). A policy statement's `Action` and `Resource` are [`StringOrStringList`](#stringorstringlist) values, which keep whether the value was written as a single string or an array.

Assign a string for a scalar value and a string array or list for an array value. A policy statement can preserve different wire shapes for `Action` and `Resource`:

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Sns;
using ByteBard.AsyncAPI.Models;

var statement = new Statement
{
    Effect = Effect.Allow,
    Principal = new PrincipalObject(new KeyValuePair<string, StringOrStringList>(
        "AWS", new SingleString("arn:aws:iam::123456789012:root"))),
    Action = "sns:Publish",
    Resource = new List<string>
    {
        "arn:aws:sns:us-east-1:123456789012:orders",
        "arn:aws:sns:us-east-1:123456789012:priority-orders",
    },
};
```

## Write

Attach an `SnsOperationBinding` at operation level and serialize the enclosing document:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Sns;
using ByteBard.AsyncAPI.Models;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Orders", Version = "1.0.0" },
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
document.Operations["publishOrders"].Bindings.Add(new SnsOperationBinding
{
    Topic = new Identifier { Name = "orders" },
});
document.Channels["orders"].Bindings.Add(new SnsChannelBinding
{
    Name = "orders",
    Policy = new Policy
    {
        Statements = new List<Statement>
        {
            new Statement
            {
                Effect = Effect.Allow,
                Principal = new PrincipalObject(new KeyValuePair<string, StringOrStringList>(
                    "AWS", new SingleString("arn:aws:iam::123456789012:root"))),
                Action = "sns:Publish",
                Resource = new List<string>
                {
                    "arn:aws:sns:us-east-1:123456789012:orders",
                    "arn:aws:sns:us-east-1:123456789012:priority-orders",
                },
            },
        },
    },
});

var yaml = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
```

## Read and use

Continuing from the Write example above, this snippet uses its `yaml` variable.

```csharp
using System;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Sns;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Sns };
var parsed = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
if (parsed.Operations["publishOrders"].Bindings.TryGetValue<SnsOperationBinding>(out var sns))
{
    Console.WriteLine(sns.Topic?.Name);
}

if (parsed.Channels["orders"].Bindings.TryGetValue<SnsChannelBinding>(out var snsChannel))
{
    var statement = snsChannel.Policy.Statements[0];
    if (statement.Action is SingleString action)
    {
        Console.WriteLine($"Single action: {action.Value}");
    }

    if (statement.Resource is StringList resources)
    {
        Console.WriteLine($"Resource list count: {resources.Values.Count}");
    }
}
```

`TryGetValue<TBinding>` returns `false` if the binding described by `TBinding.BindingKey` is absent. For present policy values, pattern-match the parsed union alternative rather than assuming a scalar or list: use `.Value` for `SingleString` and `.Values` for `StringList`.

The same collection supports `SnsChannelBinding` on a channel. These examples target AsyncAPI 3.1.0; when writing 2.x, AsyncAPI.NET also converts the document layout. Policy semantics are not evaluated by the model. See [shared setup](index.md).

## Object model

### `SnsChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Name` | `name` | `string` | Name of the topic, if different from the channel name. |
| `Ordering` | `ordering` | `Ordering` | FIFO configuration. Topics are unordered by default. |
| `Policy` | `policy` | `Policy` | Security policy of the topic. |
| `Tags` | `tags` | `Dictionary<string, string>` | AWS tags on the topic. |

### `SnsOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Topic` | `topic` | `Identifier` | The topic, when it is not the channel name, for example by ARN. |
| `Consumers` | `consumers` | `List<Consumer>` | Subscribers of the topic and their endpoints. |
| `DeliveryPolicy` | `deliveryPolicy` | `DeliveryPolicy` | Default HTTP retry policy for the topic's subscribers. |

### `Consumer`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Protocol` | `protocol` | `Protocol` | Protocol the endpoint receives messages by. |
| `Endpoint` | `endpoint` | `Identifier` | Endpoint messages are delivered to. |
| `FilterPolicy` | `filterPolicy` | `AsyncApiAny` | Filter that selects which messages the subscriber receives. |
| `FilterPolicyScope` | `filterPolicyScope` | `FilterPolicyScope` | Whether `FilterPolicy` applies to message attributes or the message body. |
| `RawMessageDelivery` | `rawMessageDelivery` | `bool` | Whether SNS attributes are removed from the body. |
| `RedrivePolicy` | `redrivePolicy` | `RedrivePolicy` | Dead-letter queue for messages that cannot be delivered. |
| `DeliveryPolicy` | `deliveryPolicy` | `DeliveryPolicy` | HTTP retry policy for this subscriber, overriding the topic's. |
| `DisplayName` | `displayName` | `string` | Display name of the subscription. |

### `Identifier`

Identifies an endpoint or topic; set the one property that applies.

| Property | YAML field | Type |
| --- | --- | --- |
| `Url` | `url` | `string` |
| `Email` | `email` | `string` |
| `Phone` | `phone` | `string` |
| `Arn` | `arn` | `string` |
| `Name` | `name` | `string` |

### `Ordering`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Type` | `type` | `OrderingType` | Standard or FIFO topic. |
| `ContentBasedDeduplication` | `contentBasedDeduplication` | `bool` | Whether messages are deduplicated by content. |

### `DeliveryPolicy`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `MinDelayTarget` | `minDelayTarget` | `int?` | Minimum delay before a retry, in seconds. |
| `MaxDelayTarget` | `maxDelayTarget` | `int?` | Maximum delay before a retry, in seconds. |
| `NumRetries` | `numRetries` | `int?` | Total number of retries. |
| `NumNoDelayRetries` | `numNoDelayRetries` | `int?` | Immediate retries, without delay. |
| `NumMinDelayRetries` | `numMinDelayRetries` | `int?` | Retries with the minimum delay. |
| `NumMaxDelayRetries` | `numMaxDelayRetries` | `int?` | Retries with the maximum delay, after backoff. |
| `BackoffFunction` | `backoffFunction` | `BackoffFunction` | Backoff algorithm between retries. |
| `MaxReceivesPerSecond` | `maxReceivesPerSecond` | `int?` | Maximum deliveries per second, per subscription. |

### `RedrivePolicy`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `DeadLetterQueue` | `deadLetterQueue` | `Identifier` | SQS dead-letter queue for messages that cannot be processed. |
| `MaxReceiveCount` | `maxReceiveCount` | `int?` | Deliveries to the source queue before a message is moved to the dead-letter queue. |

### `Policy` and `Statement`

`Policy` has one property, `Statements` (`statements`, `List<Statement>`).

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Effect` | `effect` | `Effect` | Whether the statement allows or denies access. |
| `Principal` | `principal` | `Principal` | Accounts or resources the statement applies to. |
| `Action` | `action` | `StringOrStringList` | Permissions allowed or denied, such as `sns:Publish`. |
| `Resource` | `resource` | `StringOrStringList?` | Resources the statement applies to. |
| `Condition` | `condition` | `Condition` | Conditions under which the statement applies. |

### `Principal`

| Alternative | Holds | Read from |
| --- | --- | --- |
| `PrincipalStar` | `Value` (`string`, always `*`) | `*` |
| `PrincipalObject` | `Value` (`KeyValuePair<string, StringOrStringList>`) | an object with one `AWS` or `Service` entry |

Any other principal fails to read.

### `Condition`

`Value` (`Dictionary<string, Dictionary<string, StringOrStringList>>`) maps each condition operator, such as `StringEquals`, to its keys and values.

### `StringOrStringList`

Shared with the [SQS binding](sqs.md). A `string`, `string[]` or `List<string>` converts implicitly, and the value is written back in the same shape.

| Alternative | Holds | Read from |
| --- | --- | --- |
| `SingleString` | `Value` (`string`) | a string |
| `StringList` | `Values` (`IReadOnlyList<string>`) | an array. The input is copied, and null items are rejected. |

### Enums

| Enum | Values (YAML) |
| --- | --- |
| `Protocol` | `http`, `https`, `email`, `email-json`, `sms`, `sqs`, `application`, `lambda`, `firehose` |
| `FilterPolicyScope` | `MessageAttributes`, `MessageBody` |
| `BackoffFunction` | `arithmetic`, `exponential`, `geometric`, `linear` |
| `OrderingType` | `standard`, `FIFO` |
| `Effect` | `Allow`, `Deny` |
