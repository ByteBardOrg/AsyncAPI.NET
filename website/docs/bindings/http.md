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

Operation metadata includes `Method` (string) and `Query` (`AsyncApiJsonSchema`, including schema references). Message metadata includes `Headers` (`AsyncApiJsonSchema`, including schema references) and nullable `StatusCode` (`int?`). Attach message metadata through the message's `Bindings` dictionary. `statusCode` is a V3 message-binding field; the V2 serializer omits it. The HTTP operation `type` is inferred from the operation action when serializing AsyncAPI V2 (`send` becomes `request`, `receive` becomes `response`); V3 omits `type`. The V2 reader uses that field to infer the operation action, so V3 operation input/readback needs no `type` field.

`Method` is a string rather than an enum and HTTP method values are not validated. On reading, a message `statusCode` outside the inclusive range 100–599 produces a diagnostic, but the value is retained in the binding.
