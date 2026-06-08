# Schemas

AsyncAPI.Net supports 3 types of message payloads:

1. [Schema Object](https://v2.asyncapi.com/docs/reference/specification/v2.6.0#schemaObject)
2. [Avro 1.9.0](https://avro.apache.org/docs/1.9.0/spec.html#schemas)
3. Custom formats

The payload types are `AsyncApiJsonSchema` and `AsyncApiAvroSchema` respectively.

Note that `AsyncApiJsonSchema` is implicitly convertable to `AsyncMultiFormatSchema`.

## JsonSchema

```csharp
new AsyncApiJsonSchema()
{
    Title = "title1",
    AllOf = new List<AsyncApiJsonSchema>
    {
        new AsyncApiJsonSchema
        {
            Title = "title2",
            Properties = new Dictionary<string, AsyncApiJsonSchema>
            {
                ["property1"] = new AsyncApiJsonSchema
                {
                    Type = SchemaType.Integer,
                },
                ["property2"] = new AsyncApiJsonSchema
                {
                    Type = SchemaType.String,
                    MaxLength = 15,
                },
            },
        },
        new AsyncApiJsonSchema
        {
            Title = "title3",
            Properties = new Dictionary<string, AsyncApiJsonSchema>
            {
                ["property3"] = new AsyncApiJsonSchema
                {
                    Properties = new Dictionary<string, AsyncApiJsonSchema>
                    {
                        ["property4"] = new AsyncApiJsonSchema
                        {
                            Type = SchemaType.Boolean,
                        },
                    },
                },
                ["property5"] = new AsyncApiJsonSchema
                {
                    Type = SchemaType.String,
                    MinLength = 2,
                },
            },
            Nullable = true,
        },
    },
    Nullable = true,
    ExternalDocs = new AsyncApiExternalDocumentation
    {
        Url = new Uri("http://example.com/externalDocs"),
    },
};
```

## Avro

Due to the nature of Avro, the payload type has a helper method `bool TryGetAs<T>(out T schema)` to make the casting logic slightly easier on you.

The Avro types are implemented through a common base class `AsyncApiAvroSchema`. As Avro supports adding custom properties to the schemas as "metadata", these when deserialized, will be added to the `Metadata` dictionary which exists on all implemented Avro types.

Supported types:

* Record
* Fixed
* Enum
* Union
* Map
* Array
* Primitive
* Field
* Named type

Note, all above types are prefixed "Avro" within the dotnet classes.

### Named Types

Avro named schemas can be referenced by name using `AvroNamedType`. This is separate from AsyncAPI `$ref`; Avro named references serialize as plain strings such as `"LongList"`, not as `$ref` objects.

Named types are resolved using the Avro name and namespace rules. A named schema must be defined before it is referenced, following Avro's depth-first, left-to-right traversal rule. If a name cannot be resolved, validation reports an `AsyncApiValidatorWarning` instead of a validation error.

Named references can also be used recursively and inside schemas such as arrays, unions, and map values.

```csharp
new AvroRecord
{
    Name = "LongList",
    Fields = new List<AvroField>
    {
        new AvroField
        {
            Name = "value",
            Type = AvroPrimitiveType.Long,
        },
        new AvroField
        {
            Name = "next",
            Type = new AvroUnion
            {
                Types = new List<AsyncApiAvroSchema>
                {
                    AvroPrimitiveType.Null,
                    new AvroNamedType
                    {
                        Name = "LongList",
                    },
                },
            },
        },
    },
};
```

`AvroMap.Values` accepts any `AsyncApiAvroSchema`, not only primitive types, so maps can use named schemas as values.

```csharp
new AvroMap
{
    Values = new AvroNamedType
    {
        Name = "Address",
    },
};
```

## Usage

```csharp
new AvroRecord
{
    Name = "User",
    Namespace = "com.example",
    Fields = new List<AvroField>
    {
        new AvroField
        {
            Name = "username",
            Type = AvroPrimitiveType.String,
            Doc = "The username of the user.",
            Default = new AsyncApiAny("guest"),
            Order = AvroFieldOrder.Ascending,
        },
        new AvroField
        {
            Name = "status",
            Type = new AvroEnum
            {
                Name = "Status",
                Symbols = new List<string> { "ACTIVE", "INACTIVE", "BANNED" },
            },
            Doc = "The status of the user.",
        },
        new AvroField
        {
            Name = "emails",
            Type = new AvroArray
            {
                Items = AvroPrimitiveType.String,
            },
            Doc = "A list of email addresses.",
        },
        new AvroField
        {
            Name = "metadata",
            Type = new AvroMap
            {
                Values = AvroPrimitiveType.String,
            },
            Doc = "Metadata associated with the user.",
        },
        new AvroField
        {
            Name = "address",
            Type = new AvroRecord
            {
                Name = "Address",
                Fields = new List<AvroField>
                {
                    new AvroField { Name = "street", Type = AvroPrimitiveType.String },
                    new AvroField { Name = "city", Type = AvroPrimitiveType.String },
                    new AvroField { Name = "zipcode", Type = AvroPrimitiveType.String },
                },
            },
            Doc = "The address of the user.",
        },
        new AvroField
        {
            Name = "profilePicture",
            Type = new AvroFixed
            {
                Name = "ProfilePicture",
                Size = 256,
            },
            Doc = "A fixed-size profile picture.",
        },
        new AvroField
        {
            Name = "contact",
            Type = new AvroUnion
            {
                Types = new List<AsyncApiAvroSchema>
                {
                    AvroPrimitiveType.Null,
                    new AvroRecord
                    {
                        Name = "PhoneNumber",
                        Fields = new List<AvroField>
                        {
                            new AvroField { Name = "countryCode", Type = AvroPrimitiveType.Int },
                            new AvroField { Name = "number", Type = AvroPrimitiveType.String },
                        },
                    },
                },
            },
            Doc = "The contact information of the user, which can be either null or a phone number.",
        },
    },
};
```

## Custom Formats

You can define custom payloads/formats by implementing `ISchemaParser` and attaching it via to the readers settings.

```csharp
var settings = new AsyncApiReaderSettings();
settings.SchemaParserRegistry.RegisterParser(new CustomParser());

var reader = new AsyncApiStringReader(settings);
```

There are a few moving parts that needs to align. Mainly you will need a model to hold the values and the parser itself.

### The Model

**Note: If you don't need to serialize the model, meaning only read and not write it, you don't have to implement the `SerializeV` methods.**

The typed nature of AsyncAPI.NET is what sets it apart, so of course you need to implement a model to hold the values.

The following is an excerpt from the JSONSchema implementation.

```csharp
public class AsyncApiJsonSchema : IAsyncApiSchema
{
    public string Title { get; set; }
    public SchemaType? Type { get; set; }
    public ISet<string> Required { get; set; } = new HashSet<string>();
    public double? Maximum { get; set; }
    public IList<AsyncApiJsonSchema> AllOf { get; set; } = new List<AsyncApiJsonSchema>();
    public AsyncApiJsonSchema If { get; set; }
    public IDictionary<string, AsyncApiJsonSchema> Properties { get; set; } = new Dictionary<string, AsyncApiJsonSchema>();
    public IList<AsyncApiAny> Enum { get; set; } = new List<AsyncApiAny>();
    public AsyncApiAny Const { get; set; }
    public bool Nullable { get; set; }

    public void SerializeV2(IAsyncApiWriter writer)
    {
        this.SerializeCore(writer);
    }

    public void SerializeV3(IAsyncApiWriter writer)
    {
        this.SerializeCore(writer);
    }

    private void SerializeCore(IAsyncApiWriter writer)
    {
        writer.WriteStartObject();

        // title
        writer.WriteOptionalProperty(AsyncApiConstants.Title, this.Title);

        // type
        if (this.Type != null)
        {
            var types = EnumExtensions.GetFlags<SchemaType>(this.Type.Value);
            if (types.Count() == 1)
            {
                writer.WriteOptionalProperty(AsyncApiConstants.Type, types.First().GetDisplayName());
            }
            else
            {
                writer.WriteOptionalCollection(AsyncApiConstants.Type, types.Select(t => t.GetDisplayName()), (w, s) => w.WriteValue(s));
            }
        }

        // maximum
        writer.WriteOptionalProperty(AsyncApiConstants.Maximum, this.Maximum);

        // allOf
        writer.WriteOptionalCollection(AsyncApiConstants.AllOf, this.AllOf, (w, s) => s.SerializeV2(w));

        // uniqueItems
        writer.WriteOptionalProperty(AsyncApiConstants.UniqueItems, this.UniqueItems);

        // properties
        writer.WriteOptionalMap(AsyncApiConstants.Properties, this.Properties, (w, s) => s.SerializeV2(w));

        // enum
        writer.WriteOptionalCollection(AsyncApiConstants.Enum, this.Enum, (nodeWriter, s) => nodeWriter.WriteAny(s));

        writer.WriteOptionalObject(AsyncApiConstants.Const, this.Const, (w, s) => w.WriteAny(s));

        // nullable
        writer.WriteOptionalProperty(AsyncApiConstants.Nullable, this.Nullable, false);

        writer.WriteEndObject();
    }
}
```

So basically. Define the properties. Define how they are serialized.

### The Parser

Deserializers/parsers in AsyncAPI.NET uses maps of fields that takes an `Action<T>` that tells it how to get the proper value out.
You can see an example of this below (excerpt from the JsonSchemaDeserializer).

```csharp
private static readonly FixedFieldMap<AsyncApiJsonSchema> schemaFixedFields = new()
{
    {
        "title", (a, n) => { a.Title = n.GetScalarValue(); }
    },
    {
        "type", (a, n) => { a.Type = n.GetScalarValue().GetEnumFromDisplayName<SchemaType>(); }
    },
    {
        "required",
        (a, n) => { a.Required = new HashSet<string>(n.CreateSimpleList(n2 => n2.GetScalarValue())); }
    },
    {
        "maximum",
        (a, n) =>
        {
            a.Maximum = double.Parse(n.GetScalarValue(), NumberStyles.Float, n.Context.Settings.CultureInfo);
        }
    },
    {
        "uniqueItems", (a, n) => { a.UniqueItems = bool.Parse(n.GetScalarValue()); }
    },
    {
        "enum", (a, n) => { a.Enum = n.CreateListOfAny(); }
    },
    {
        "const", (a, n) => { a.Const = n.CreateAny(); }
    },
    {
        "if", (a, n) => { a.If = LoadSchema(n); }
    },
    {
        "properties", (a, n) => { a.Properties = n.CreateMap(LoadSchema); }
    },
    {
        "allOf", (a, n) => { a.AllOf = n.CreateList(LoadSchema); }
    },
    {
        "nullable", (a, n) => { a.Nullable = n.GetBooleanValue(); }
    },
};
```

You'll notice its all just field names from the schema and an action that takes the `ParseNode` and creates the proper structure depending on the type of property. There are extensions for maps, collections, scalars etc.

The `ISchemaParser` defines 2 methods.

1. `IAsyncApiSchema LoadSchema(ParseNode node)`
2. `IEnumerable<string> SupportedFormats`

The first should generally look something like this:

```csharp
public IAsyncApiSchema LoadSchema(ParseNode node)
{
    // Check that we are dealing with an object and cast accordingly.
    var mapNode = node.CheckMapNode("arbitrary string");

    var schema = new MyCustomSchema();

    // map each propery against the fixedFieldMap
    foreach (var property in mapNode)
    {
        property.ParseField(schema, schemaFixedFields, null);
    }

    return schema;
}
```

The latter should simply be the list of formats that should resolve to this schema. For JsonSchema its looks like this:

```csharp
public IEnumerable<string> SupportedFormats => new List<string>
{
    "application/vnd.aai.asyncapi+json",
    "application/vnd.aai.asyncapi+yaml",
    "application/vnd.aai.asyncapi",
    "application/schema+json;version=draft-07",
    "application/schema+yaml;version=draft-07",
}
```

And that is all you need to implement a custom schema parser.

You can check out a full example in the following Unit Test: https://github.com/ByteBardOrg/AsyncAPI.NET/blob/vnext/test/ByteBard.AsyncAPI.Tests/Models/CustomSchema_Should.cs
