---
sidebar_position: 13
title: ROS 2 bindings
description: Read and write ROS 2 server and operation bindings.
---

# ROS 2

`BindingsCollection.ROS2` registers server and operation parsers.

```csharp
using ByteBard.AsyncAPI.Bindings.ROS2;
using ByteBard.AsyncAPI.Models;

var operation = new AsyncApiOperation();
operation.Bindings.Add(new ROS2OperationBinding
{
    Role = "publisher",
    Node = "orders",
    QosPolicies = new ROS2QosPolicies
    {
        Reliability = "reliable",
        History = "keep_last",
        Durability = "transient_local",
        Lifespan = 0,
        Deadline = -1,
        Liveliness = "automatic",
        LeaseDuration = 0,
    },
});

var server = new AsyncApiServer();
server.Bindings.Add(new ROS2ServerBinding
{
    RmwImplementation = "rmw_fastrtps_cpp",
    DomainId = 42,
});
```

`Bindings.Add(binding)` stores the binding under its `BindingKey` (`ros2`) for either model level.

The properties are listed in the [object model](#object-model).

## Write

Attach the typed operation binding at the operation level, then serialize the enclosing document:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings.ROS2;
using ByteBard.AsyncAPI.Models;
using System.Collections.Generic;

var document = new AsyncApiDocument
{
    Info = new AsyncApiInfo { Title = "Camera", Version = "1.0.0" },
    Channels = new Dictionary<string, AsyncApiChannel>
    {
        ["camera"] = new AsyncApiChannel { Address = "camera" },
    },
    Operations = new Dictionary<string, AsyncApiOperation>
    {
        ["publishCamera"] = new AsyncApiOperation
        {
            Action = AsyncApiAction.Send,
            Channel = new AsyncApiChannelReference("#/channels/camera"),
        },
    },
};
document.Operations["publishCamera"].Bindings.Add(new ROS2OperationBinding
{
    Role = "publisher",
    Node = "camera",
    QosPolicies = new ROS2QosPolicies
    {
        Reliability = "reliable",
        History = "keep_last",
        Durability = "transient_local",
        Lifespan = 0,
        Deadline = -1,
        Liveliness = "automatic",
        LeaseDuration = 0,
    },
});

var yaml = document.SerializeAsYaml(AsyncApiVersion.AsyncApi3_0);
```

## Read and use

Continuing from the Write example above, this snippet uses its `yaml` variable.

```csharp
using System;
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.ROS2;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.ROS2 };
var parsed = new AsyncApiStringReader(settings).Read(yaml, out var diagnostics);
if (parsed.Operations["publishCamera"].Bindings.TryGetValue<ROS2OperationBinding>(out var ros2))
{
    Console.WriteLine($"{ros2.Role} on {ros2.Node}, deadline={ros2.QosPolicies.Deadline}");
}

var parsedServer = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>("""
    bindings:
      ros2:
        rmwImplementation: rmw_fastrtps_cpp
        domainId: 42
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedServer.Bindings.TryGetValue<ROS2ServerBinding>(out var ros2Server))
{
    Console.WriteLine($"{ros2Server.RmwImplementation}, domain={ros2Server.DomainId}");
}
```

The typed lookups return `false` if their server or operation binding is absent.

The ROS 2 server and operation serializers omit `BindingVersion`, even when set. These examples target AsyncAPI 3.1.0; AsyncAPI 2.x serialization converts the surrounding document layout. Policy strings and timing values are not validated against the selected RMW implementation. See [shared reader/writer setup](index.md).

## Object model

### `ROS2ServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `RmwImplementation` | `rmwImplementation` | `string` | ROS middleware implementation, such as `rmw_fastrtps_cpp`. |
| `DomainId` | `domainId` | `int?` | ROS domain ID. |

### `ROS2OperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Role` | `role` | `string` | Role of the node in the communication, such as `publisher`, `subscriber`, `service_client` or `action_server`. |
| `Node` | `node` | `string` | Name of the node. |
| `QosPolicies` | `qosPolicies` | `ROS2QosPolicies` | Quality of service policies. |

### `ROS2QosPolicies`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Reliability` | `reliability` | `string` | `best_effort` or `reliable`. |
| `History` | `history` | `string` | `keep_last`, `keep_all` or `unknown`. |
| `Durability` | `durability` | `string` | `transient_local` or `volatile`. |
| `Lifespan` | `lifespan` | `int?` | Maximum age of a message, in milliseconds. |
| `Deadline` | `deadline` | `int?` | Expected maximum time between messages, in milliseconds. |
| `Liveliness` | `liveliness` | `string` | `automatic` or `manual_by_topic`. |
| `LeaseDuration` | `leaseDuration` | `int?` | Time before a publisher is considered no longer alive, in milliseconds. |
