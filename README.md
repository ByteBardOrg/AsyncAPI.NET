# AsyncAPI.NET

[![Build & Test](https://github.com/ByteBardOrg/AsyncAPI.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/ByteBardOrg/AsyncAPI.NET/actions/workflows/ci.yml)

AsyncAPI.NET provides a .NET object model for AsyncAPI documents, JSON/YAML readers and writers, and protocol bindings.

It supports AsyncAPI 2.x and 3.x. V2 documents are upgraded to the V3-shaped model when read and can be written back as V2. Serialization currently emits **2.6** or **3.1.0**; the V3 selector is still named `AsyncApiVersion.AsyncApi3_0`.

See the **[documentation site](https://bytebard.org/AsyncAPI.NET/)** for guides and examples, or the [changelog](CHANGELOG.md) for release notes.

## Install

Install the core model and serializers, plus the readers for JSON/YAML input:

```sh
dotnet add package ByteBard.AsyncAPI.NET
dotnet add package ByteBard.AsyncAPI.NET.Readers
```

For protocol bindings, also install:

```sh
dotnet add package ByteBard.AsyncAPI.NET.Bindings
```

Binding readers require explicit registration; see [protocol bindings](https://bytebard.org/AsyncAPI.NET/docs/bindings).

## Quick start

Parse YAML, check diagnostics, and write JSON (C# 11 or later):

```csharp
using System;
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Readers;

var source = """
asyncapi: 3.1.0
info:
  title: Example API
  version: 1.0.0
channels: {}
""";

var document = new AsyncApiStringReader().Read(source, out var diagnostics);

foreach (var error in diagnostics.Errors)
    Console.Error.WriteLine($"Error at {error.Pointer}: {error.Message}");
foreach (var warning in diagnostics.Warnings)
    Console.Error.WriteLine($"Warning at {warning.Pointer}: {warning.Message}");

if (diagnostics.Errors.Count > 0)
    return;

Console.WriteLine(document.SerializeAsJson(AsyncApiVersion.AsyncApi3_0));
```

## Documentation

- [Getting started](https://bytebard.org/AsyncAPI.NET/docs/guides/getting-started)
- [Reading documents and diagnostics](https://bytebard.org/AsyncAPI.NET/docs/guides/reading)
- [Schemas](https://bytebard.org/AsyncAPI.NET/docs/guides/schemas): [JSON Schema](https://bytebard.org/AsyncAPI.NET/docs/guides/json-schema) and [Avro](https://bytebard.org/AsyncAPI.NET/docs/guides/avro)
- [References and resolution](https://bytebard.org/AsyncAPI.NET/docs/guides/references)
- [Protocol bindings](https://bytebard.org/AsyncAPI.NET/docs/bindings)
- Extending: [extension parsers](https://bytebard.org/AsyncAPI.NET/docs/guides/custom-extension-parsers), [schema parsers](https://bytebard.org/AsyncAPI.NET/docs/guides/custom-schema-parsers), [bindings](https://bytebard.org/AsyncAPI.NET/docs/guides/custom-bindings), and [validation rules](https://bytebard.org/AsyncAPI.NET/docs/guides/validation-rules)

## Contributing

Contributions and suggestions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) and the [documentation development instructions](website/README.md).

## Attribution

- [OpenAPI.NET](https://github.com/microsoft/OpenAPI.NET) — [MIT License](https://github.com/microsoft/OpenAPI.NET/blob/vnext/LICENSE)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) — [MIT License](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt)

## License

[Apache 2.0](LICENSE.txt)
