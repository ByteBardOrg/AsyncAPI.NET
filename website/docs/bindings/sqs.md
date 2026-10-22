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

Channel fields are `Queue` and `DeadLetterQueue`. A queue includes name, FIFO/deduplication/throughput settings, delivery delay, visibility timeout, receive wait time, retention period, redrive policy, policy, and tags. Operation fields: `Queues` (`List<Queue>`). Policies contain statements with effect, principal, action, resource, and condition.

`Statement.Action` is the abstract `StringOrStringList` type; nullable `Resource` uses the same type. Its concrete alternatives are `SingleString` (property `Value`) and `StringList` (property `Values`). Strings, `string[]`, and `List<string>` convert implicitly, preserving the scalar-versus-array wire shape without an `AsyncApiAny` wrapper. `StringList` copies its input and rejects null items. SNS uses the same hierarchy (see [SNS](sns.md)).

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
