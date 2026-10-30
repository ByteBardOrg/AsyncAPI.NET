---
sidebar_position: 16
title: Amazon SQS bindings
description: Read and write SQS channel and operation bindings.
---

# Amazon SQS

`BindingsCollection.Sqs` registers channel and operation parsers.

```csharp
using ByteBard.AsyncAPI.Bindings.Sqs;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new SqsChannelBinding
{
    Queue = new Queue { Name = "orders", VisibilityTimeout = 30 },
});
```

`Bindings.Add(binding)` uses the binding's `BindingKey` (`sqs`) rather than a manually supplied dictionary key.

The properties are listed in the [object model](#object-model). A policy statement's `Action` and `Resource` are [`StringOrStringList`](sns.md#stringorstringlist) values, shared with SNS, which keep whether the value was written as a single string or an array.

Assign a string for a scalar value and a string array or list for an array value. For example, this uses a scalar `Action` and list-valued `Resource`:

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Sqs;
using ByteBard.AsyncAPI.Models;

var statement = new Statement
{
    Effect = Effect.Allow,
    Principal = new PrincipalObject(new KeyValuePair<string, StringOrStringList>(
        "AWS", new SingleString("arn:aws:iam::123456789012:root"))),
    Action = "sqs:SendMessage",
    Resource = new List<string>
    {
        "arn:aws:sqs:us-east-1:123456789012:orders",
        "arn:aws:sqs:us-east-1:123456789012:priority-orders",
    },
};
```

## Write

Attach the typed channel binding to its channel, then serialize the enclosing document:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Sqs;
using ByteBard.AsyncAPI.Models;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Orders", Version = "1.0.0" },
    Channels = new Dictionary<string, AsyncApiChannel>
    {
        ["orders"] = new AsyncApiChannel { Address = "orders" },
    },
};
document.Channels["orders"].Bindings.Add(new SqsChannelBinding
{
    Queue = new Queue
    {
        Name = "orders",
        VisibilityTimeout = 30,
        Policy = new Policy
        {
            Statements = new List<Statement>
            {
                new Statement
                {
                    Effect = Effect.Allow,
                    Principal = new PrincipalObject(new KeyValuePair<string, StringOrStringList>(
                        "AWS", new SingleString("arn:aws:iam::123456789012:root"))),
                    Action = "sqs:SendMessage",
                    Resource = new List<string>
                    {
                        "arn:aws:sqs:us-east-1:123456789012:orders",
                        "arn:aws:sqs:us-east-1:123456789012:priority-orders",
                    },
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
using ByteBard.AsyncAPI.Bindings.Sqs;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Sqs };
var parsed = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
if (parsed.Channels["orders"].Bindings.TryGetValue<SqsChannelBinding>(out var sqs))
{
    Console.WriteLine($"{sqs.Queue.Name}: {sqs.Queue.VisibilityTimeout}");

    var statement = sqs.Queue.Policy.Statements[0];
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

`TryGetValue<TBinding>` returns `false` if the binding described by `TBinding.BindingKey` is absent. For present policy values, pattern-match the parsed union alternative instead of assuming a scalar or list: use `.Value` for `SingleString` and `.Values` for `StringList`.

The same collection supports `SqsOperationBinding` on operations. These examples target AsyncAPI 3.1.0; AsyncAPI.NET also converts document layout when writing 2.x. Queue and IAM policy validity is not checked against AWS. See [shared setup](index.md).

## Object model

### `SqsChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Queue` | `queue` | `Queue` | The queue used as the channel. |
| `DeadLetterQueue` | `deadLetterQueue` | `Queue` | The queue for messages that cannot be processed. |

### `SqsOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Queues` | `queues` | `List<Queue>` | Queues that are the endpoint of an SNS operation binding, or the dead-letter queue of this binding. |

### `Queue`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Name` | `name` | `string` | Name of the queue. An SNS operation binding refers to the queue by this name. |
| `FifoQueue` | `fifoQueue` | `bool` | Whether this is a FIFO queue. |
| `DeduplicationScope` | `deduplicationScope` | `DeduplicationScope?` | Whether deduplication applies per message group or per queue. |
| `FifoThroughputLimit` | `fifoThroughputLimit` | `FifoThroughputLimit?` | Whether the FIFO throughput quota applies per queue or per message group. |
| `DeliveryDelay` | `deliveryDelay` | `int?` | Seconds before a new message can be received. |
| `VisibilityTimeout` | `visibilityTimeout` | `int?` | Seconds a received message is hidden from other consumers. |
| `ReceiveMessageWaitTime` | `receiveMessageWaitTime` | `int?` | Seconds a receive waits for messages. `0` means short polling. |
| `MessageRetentionPeriod` | `messageRetentionPeriod` | `int?` | Seconds a message is retained, unless deleted. |
| `RedrivePolicy` | `redrivePolicy` | `RedrivePolicy` | Dead-letter queue for messages that cannot be processed. |
| `Policy` | `policy` | `Policy` | Security policy of the queue. |
| `Tags` | `tags` | `Dictionary<string, string>` | AWS tags on the queue. |

### `RedrivePolicy`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `DeadLetterQueue` | `deadLetterQueue` | `Identifier` | The dead-letter queue. |
| `MaxReceiveCount` | `maxReceiveCount` | `int?` | Deliveries to the source queue before a message is moved to the dead-letter queue. |

### `Identifier`

Identifies a queue; set the one property that applies.

| Property | YAML field | Type |
| --- | --- | --- |
| `Arn` | `arn` | `string` |
| `Name` | `name` | `string` |

### `Policy` and `Statement`

`Policy` has one property, `Statements` (`statements`, `List<Statement>`). `Principal`, `Condition` and `StringOrStringList` work as in the [SNS binding](sns.md#principal).

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Effect` | `effect` | `Effect` | Whether the statement allows or denies access. |
| `Principal` | `principal` | `Principal` | Accounts or resources the statement applies to. |
| `Action` | `action` | `StringOrStringList` | Permissions allowed or denied, such as `sqs:SendMessage`. |
| `Resource` | `resource` | `StringOrStringList?` | Resources the statement applies to. |
| `Condition` | `condition` | `Condition` | Conditions under which the statement applies. |

### Enums

| Enum | Values (YAML) |
| --- | --- |
| `DeduplicationScope` | `queue`, `messageGroup` |
| `FifoThroughputLimit` | `perQueue`, `perMessageGroupId` |
| `Effect` | `allow`, `deny` |
