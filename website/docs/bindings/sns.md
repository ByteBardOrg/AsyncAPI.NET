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

Channel fields are `Name`, `Ordering`, `Policy`, and string-valued `Tags`. Operation fields are `Topic` (`Identifier`), `Consumers` (`List<Consumer>`), and `DeliveryPolicy`. Nested models cover policy statements, consumer protocol/endpoint/filter/redrive/delivery configuration, and ordering configuration.

`Statement.Action` is the abstract `StringOrStringList` type; nullable `Resource` uses the same type. Its concrete alternatives are `SingleString` (property `Value`) and `StringList` (property `Values`). String scalars, `string[]`, and `List<string>` convert implicitly, preserving the scalar-versus-array wire shape without an `AsyncApiAny` wrapper. `StringList` copies its input and rejects null items. The same hierarchy is used by SQS policies.

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
