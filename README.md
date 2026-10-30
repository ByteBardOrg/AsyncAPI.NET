# AsyncAPI.NET
This serves as the official future of AsyncAPI.NET.
To be able to give the level of support I want, I have had to fork my original work from the The LEGO Group, and create my own.
This is unfortunate, but after much back and not a lot of forth, I have decided that it was time.

[![Build & Test](https://github.com/ByteBardOrg/AsyncAPI.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/ByteBardOrg/AsyncAPI.NET/actions/workflows/ci.yml)


The AsyncAPI.NET SDK contains a useful object model for the AsyncAPI specification in .NET along with common serializers to extract raw AsyncAPI JSON and YAML documents from the model as well.
AsyncAPI.NET supports the current releases of the AsyncAPI 2.x and 3.x major versions. Today, serialization emits 2.6 or 3.1.0 respectively.
V2 documents are upgraded to the V3 model during deserialization and can be written back as V2 (see writing examples). The V3 serialization selector is still named `AsyncApiVersion.AsyncApi3_0`.
  
[CHANGELOG](https://github.com/ByteBardOrg/AsyncAPI.NET/blob/vnext/CHANGELOG.md)  
[Getting started guide](https://bytebard.org/AsyncAPI.NET/docs/guides/getting-started)

## Documentation
The [documentation site](https://bytebard.org/AsyncAPI.NET/) is built with Docusaurus. See [website/README.md](website/README.md) for local development instructions.

## Installation
Generally you want to use Readers and Bindings.
They have however been split to allow for different scenarios without polluting with unnecesary packages.

Install the NuGet packages:
### ByteBard.AsyncAPI.NET
[![Nuget](https://img.shields.io/nuget/v/ByteBard.AsyncAPI.NET?label=ByteBard.AsyncAPI.NET&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET/)  
[![Nuget](https://img.shields.io/nuget/vpre/ByteBard.AsyncAPI.NET?label=ByteBard.AsyncAPI.NET-Preview&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET/)  

### ByteBard.AsyncAPI.Readers
[![Nuget](https://img.shields.io/nuget/v/ByteBard.AsyncAPI.NET.Readers?label=ByteBard.AsyncAPI.Readers&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET.Readers/)  
[![Nuget](https://img.shields.io/nuget/vpre/ByteBard.AsyncAPI.NET.Readers?label=ByteBard.AsyncAPI.Readers-Preview&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET.Readers/)  

### ByteBard.AsyncAPI.Bindings
[![Nuget](https://img.shields.io/nuget/v/ByteBard.AsyncAPI.NET.Bindings?label=ByteBard.AsyncAPI.Bindings&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET.Bindings/)  
[![Nuget](https://img.shields.io/nuget/vpre/ByteBard.AsyncAPI.NET.Bindings?label=ByteBard.AsyncAPI.Bindings-Preview&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET.Bindings/)  

# Example Usage
Main classes to know:

* AsyncApiStringReader
* AsyncApiStringWriter
  * There is an extension on the AsyncApiDocument type which allows Serializing as well (`new AsyncApiDocument().SerializeAsJson()` or `new AsyncApiDocument().SerializeAsYaml()`

## Writing

```csharp
var specification =
"""
asyncapi: 3.1.0
info:
  title: UsersAPI
  version: 1.0.0
  externalDocs:
    description: Find more info here
    url: https://www.asyncapi.com
  tags:
    - name: e-commerce
servers:
  production:
    host: "rabbitmq.in.mycompany.com:5672"
    pathname: "/production"
    protocol: "amqp"
channels:
  UserSignup:
    address: "user/signedup"
    messages: 
      UserMessage: 
        payload:
          type: object
          properties:
            displayName:
              type: string
              description: Name of the user
operations:
  ConsumeUserSignups:
    action: receive
    channel: 
      $ref: "#/channels/UserSignup"
""";
var reader = new AsyncApiStringReader();
var document = reader.Read(specification, out var diagnostics);
var v2Document = document.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
var v3Document = document.SerializeAsJson(AsyncApiVersion.AsyncApi3_0);
```


## Reading

There are 3 reader types
1. AsyncApiStringReader
2. AsyncApiTextReader
3. AsyncApiStreamReader

All 3 supports both json and yaml.

#### StreamReader
```csharp
var httpClient = new HttpClient
{
  BaseAddress = new Uri("https://raw.githubusercontent.com/asyncapi/spec/"),
};

var stream = await httpClient.GetStreamAsync("master/examples/streetlights-kafka.yml");
var asyncApiDocument = new AsyncApiStreamReader().Read(stream, out var diagnostic);
```

#### StringReader
```csharp
var yaml =
	"""
	asyncapi: 2.6.0
	  info:
	    title: my first asyncapi
	channels:
	  users:
	    subscribe:
	      operationId: users
	      description: my users channel
	      message:
	        $ref: '#/components/messages/MyMessage'
	components:
	  messages:
	    MyMessage:
	      name: Hello!
	""";

var asyncApiDocument = new AsyncApiStringReader().Read(yaml, out var diagnostic);
```
All readers will write warnings and errors to the diagnostics.


## Reference Resolution
Internal references are resolved by default. This includes component and non-component references e.g `#/components/messages/MyMessage` and `#/servers/production`.
External references can be resolved by setting `ReferenceResolution` to `ResolveAllReferences`.
The default implementation will resolve anything prefixed with `file://`, `http://` & `https://`, however a custom implementation can be made, by inhereting from the `IStreamLoader` interface and setting the `ExternalReferenceLoader` in the `AsyncApiReaderSettings`.
External references are always force converted to Json during resolution. This means that both yaml and json is supported - but not other serialization languages.

```csharp
var settings = new AsyncApiReaderSettings { ReferenceResolution = ReferenceResolutionSetting.ResolveAllReferences };
var document = new AsyncApiStringReader(settings).Read(json, out var diagnostics);
```

Reference resolution can be disabled by setting `ReferenceResolution` to `DoNotResolveReferences`. 

## Bindings
To add support for reading bindings, simply add the bindings you wish to support, to the `Bindings` collection of `AsyncApiReaderSettings`.
There is a nifty helper to add different types of bindings, or like in the example `All` of them.

```csharp
var settings = new AsyncApiReaderSettings();
settings.Bindings = BindingsCollection.All;
var asyncApiDocument = new AsyncApiStringReader(settings).Read(yaml, out var diagnostic);
```

## Attribution

* [OpenAPI.Net](https://github.com/microsoft/OpenAPI.NET) - [MIT License](https://github.com/microsoft/OpenAPI.NET/blob/vnext/LICENSE)
* [YamlDotNet](https://github.com/aaubry/YamlDotNet) - [MIT License](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt)
  
## Contribution

This project welcomes contributions and suggestions.
Do you want to contribute to the project? Find out how [here](CONTRIBUTING.md).

## License
[Apache 2.0](https://github.com/ByteBardOrg/AsyncAPI.NET/blob/main/LICENSE.txt)
