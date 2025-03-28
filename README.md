# AsyncAPI.NET
This serves as the official future of AsyncAPI.NET.
To be able to give the level of support I want, I have had to fork my original work from the ByteBard Group, and create my own.
This is unfortunate, but after much back and not a lot of forth, I have decided that it was time.

[![Build & Test](https://github.com/ByteBardOrg/AsyncAPI.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/ByteBardOrg/AsyncAPI.NET/actions/workflows/ci.yml)


The AsyncAPI.NET SDK contains a useful object model for the AsyncAPI specification in .NET along with common serializers to extract raw AsyncAPI JSON and YAML documents from the model as well.

[CHANGELOG](https://github.com/ByteBardOrg/AsyncAPI.NET/blob/main/CHANGELOG.md)  
[Wiki and getting started guide](https://github.com/ByteBardOrg/AsyncAPI.NET/wiki)

## Installation
Generally you wan't to use Readers and Bindings.
They have however been split to allow for different scenarios without polluting with unnecesary packages.

Install the NuGet packages:
### ByteBard.AsyncAPI.NET
[![Nuget](https://img.shields.io/nuget/v/ByteBard.AsyncAPI.NET?label=ByteBard.AsyncAPI.NET&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET/)  
[![Nuget](https://img.shields.io/nuget/vpre/ByteBard.AsyncAPI.NET?label=ByteBard.AsyncAPI.NET-Preview&style=for-the-badge)](https://www.nuget.org/packages/ByteBard.AsyncAPI.NET/)  

### ByteBard.AsyncAPI.Readers
[![Nuget](https://img.shields.io/nuget/v/ByteBard.AsyncAPI.NET.Readers?label=ByteBard.AsyncAPI.Readers&style=for-the-badge)](https://www.nuget.org/packages/AsyncAPI.NET.Readers/)  
[![Nuget](https://img.shields.io/nuget/vpre/ByteBard.AsyncAPI.NET.Readers?label=ByteBard.AsyncAPI.Readers-Preview&style=for-the-badge)](https://www.nuget.org/packages/AsyncAPI.NET.Readers/)  

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
 var myFirstAsyncApi = new AsyncApiDocument
 {
     Info = new AsyncApiInfo
     {
         Title = "my first asyncapi",
     },
     Channels = new Dictionary<string, AsyncApiChannel>
     {
         {
             "users", new AsyncApiChannel
             {
                 Subscribe = new AsyncApiOperation
                 {
                     OperationId = "users",
                     Description = "my users channel",
                     Message = new List<AsyncApiMessage>
                     {
                       new AsyncApiMessageReference("#/components/messages/MyMessage"),
                     },
                 },
             }
         },
     },
     Components = new AsyncApiComponents
     {
         Messages = new Dictionary<string, AsyncApiMessage>
         {
             {
                 "MyMessage", new AsyncApiMessage
                 {
                     Name = "Hello!",
                 }
             },
         },
     },
 };

var yaml = myFirstAsyncApi.SerializeAsYaml(AsyncApi);

//asyncapi: 2.6.0
//  info:
//    title: my first asyncapi
//channels:
//  users:
//    subscribe:
//      operationId: users
//      description: my users channel
//      message:
//        $ref: '#/components/messages/MyMessage'
//components:
//  messages:
//    MyMessage:
//      name: Hello!
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
Internal references are resolved by default. This includes component and non-component references e.g `#/components/messages/MyMessage` and `#/servers/0`.
External references can be resolved by setting `ReferenceResolution` to `ResolveAllReferences`.
The default implementation will resolve anything prefixed with `file://`, `http://` & `https://`, however a custom implementation can be made, by inhereting from the `IStreamLoader` interface and setting the `ExternalReferenceLoader` in the `AsyncApiReaderSettings`.
External references are always force converted to Json during resolution. This means that both yaml and json is supported - but not other serialization languages.

```csharp
var settings = new AsyncApiReaderSettings { ReferenceResolution = ReferenceResolution.ResolveAllReferences };
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
* [JsonPointer.Net](https://github.com/json-everything/json-everything) - [MIT License](https://github.com/json-everything/json-everything/blob/master/LICENSE)
  
## Contribution

This project welcomes contributions and suggestions.
Do you want to contribute to the project? Find out how [here](CONTRIBUTING.md).

## License
[Apache 2.0](https://github.com/ByteBardOrg/AsyncAPI.NET/blob/main/LICENSE.txt)
