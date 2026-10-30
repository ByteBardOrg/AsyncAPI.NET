---
sidebar_position: 5
title: HTTP bindings
description: Read and write HTTP operation and message bindings.
---

# HTTP

The HTTP collection registers operation and message parsers only: `HttpOperationBinding` and `HttpMessageBinding`. Configure reading with `BindingsCollection.Http` (or `All`) in reader settings.

## Write

HTTP method/query metadata belongs on an operation; response status and headers belong on a message:

```csharp
using ByteBard.AsyncAPI.Bindings.Http;
using ByteBard.AsyncAPI.Models;

var operation = new AsyncApiOperation();
operation.Bindings.Add(new HttpOperationBinding
{
    Method = "POST",
    Query = new AsyncApiJsonSchema
    {
        Type = SchemaType.Object,
        Properties =
        {
            ["page"] = new AsyncApiJsonSchema { Type = SchemaType.Integer },
        },
    },
});

var message = new AsyncApiMessage();
message.Bindings.Add(new HttpMessageBinding
{
    Headers = new AsyncApiJsonSchemaReference("#/components/schemas/responseHeaders"),
    StatusCode = 201,
});
```

`Query` and `Headers` can each be assigned an inline `AsyncApiJsonSchema` or an `AsyncApiJsonSchemaReference`.

## Read and use

Register the HTTP parsers and parse a message fragment to retrieve the message-level typed binding:

```csharp
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings;
using ByteBard.AsyncAPI.Bindings.Http;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var settings = new AsyncApiReaderSettings { Bindings = BindingsCollection.Http };
var parsed = new AsyncApiStringReader(settings).ReadFragment<AsyncApiMessage>("""
    bindings:
      http:
        statusCode: 201
    """, AsyncApiVersion.AsyncApi3_0, out var diagnostics);

if (parsed.Bindings.TryGetValue<HttpMessageBinding>(out var http))
{
    Console.WriteLine(http.StatusCode);
}

var parsedOperation = new AsyncApiStringReader(settings).ReadFragment<AsyncApiOperation>("""
    bindings:
      http:
        method: POST
        query:
          type: object
          properties:
            page:
              type: integer
    """, AsyncApiVersion.AsyncApi3_0, out diagnostics);
if (parsedOperation.Bindings.TryGetValue<HttpOperationBinding>(out var httpOperation))
{
    Console.WriteLine(httpOperation.Query.Properties["page"].Type);
}
```

## Object model

### `HttpOperationBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Method` | `method` | `string` | HTTP method, such as `GET` or `POST`. Not validated. |
| `Query` | `query` | `AsyncApiJsonSchema` | Schema of the query parameters, as an inline schema or an `AsyncApiJsonSchemaReference`. |

### `HttpMessageBinding`

| Property | YAML field | Type | Description |
| --- | --- | --- | --- |
| `Headers` | `headers` | `AsyncApiJsonSchema` | Schema of the HTTP headers, as an inline schema or an `AsyncApiJsonSchemaReference`. |
| `StatusCode` | `statusCode` | `int?` | Response status code, for messages of an operation reply. Written in V3 only. |

### Version differences

- V2 operation bindings have a `type` field, `request` or `response`. It is not a property: writing V2 derives it from the operation's action (`Send` becomes `request`, `Receive` becomes `response`), and reading V2 uses it to set the action. V3 has no `type` field.
- A `statusCode` outside 100–599 is reported in the diagnostics when reading, and kept in the binding.
