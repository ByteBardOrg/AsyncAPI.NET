---
sidebar_position: 7
title: JMS bindings
description: Read and write JMS server, channel, and message bindings.
---

# JMS

`BindingsCollection.JMS` registers server, channel, and message binding parsers.

## Write

Attach a destination definition to the channel model:

```csharp
using System.Collections.Generic;
using ByteBard.AsyncAPI.Bindings.JMS;
using ByteBard.AsyncAPI.Models;

var channel = new AsyncApiChannel();
channel.Bindings.Add(new JMSChannelBinding
{
    Destination = "orders",
    DestinationType = "queue",
});

var server = new AsyncApiServer();
server.Bindings.Add(new JMSServerBinding
{
    JmsConnectionFactory = "ConnectionFactory",
    ClientID = "worker",
    Properties = new List<JMSProperty>
    {
        new() { Name = "enabled", Value = new AsyncApiAny(false) },
        new() { Name = "priority", Value = new AsyncApiAny(3) },
        new() { Name = "region", Value = new AsyncApiAny("west") },
    },
});

var message = new AsyncApiMessage();
message.Bindings.Add(new JMSMessageBinding
{
    Headers = new AsyncApiJsonSchemaReference("#/components/schemas/jmsHeaders"),
});
```

## Read and use

Register JMS parsers and read back the typed channel binding and its destination:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.JMS;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.JMS };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiChannel>("""
    bindings:
      jms:
        destination: orders
        destinationType: queue
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<JMSChannelBinding>(out var jms))
{
    Console.WriteLine(jms.Destination);
}

var parsedServer = new AsyncApiStringReader(settings).ReadFragment<AsyncApiServer>("""
    bindings:
      jms:
        jmsConnectionFactory: ConnectionFactory
        properties:
          - name: enabled
            value: false
          - name: priority
            value: 3
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedServer.Bindings.TryGetValue<JMSServerBinding>(out var jmsServer))
{
    var enabled = jmsServer.Properties[0].Value.GetValue<bool>();
    var priority = jmsServer.Properties[1].Value.GetValue<decimal>();
    Console.WriteLine($"{enabled}, {priority}");
}

var parsedMessage = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>("""
    bindings:
      jms:
        headers:
          $ref: '#/components/schemas/jmsHeaders'
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedMessage.Bindings.TryGetValue<JMSMessageBinding>(out var jmsMessage)
    && jmsMessage.Headers is AsyncApiJsonSchemaReference headersReference)
{
    Console.WriteLine(headersReference.Reference.Reference);
}
```

## Object model

Destination types and property values are not validated against a broker.

### `JMSServerBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `JmsConnectionFactory` | `jmsConnectionFactory` | `string` | Fully qualified class name of the connection factory. |
| `Properties` | `properties` | `IList<JMSProperty>` | Additional properties of the connection factory. |
| `ClientID` | `clientID` | `string` | Client identifier for durable subscriptions. |

### `JMSChannelBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Destination` | `destination` | `string` | Name of the destination, if different from the channel address. |
| `DestinationType` | `destinationType` | `string` | `queue` or `fifo-queue`. |

### `JMSMessageBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Headers` | `headers` | `AsyncApiJsonSchema` | Schema of the JMS headers, as an inline schema or an `AsyncApiJsonSchemaReference`. |

### `JMSProperty`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Name` | `name` | `string` | Name of the property. |
| `Value` | `value` | `AsyncApiAny` | Value of the property. Read it with `Value.GetValue<T>()`. |
