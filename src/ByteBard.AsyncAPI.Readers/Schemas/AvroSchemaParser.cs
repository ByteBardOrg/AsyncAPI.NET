namespace ByteBard.AsyncAPI.Readers;

using System.Collections.Generic;
using Models.Interfaces;
using ParseNodes;

public class AvroSchemaParser : IAsyncApiSchemaParser
{
    public IAsyncApiSchema LoadSchema(ParseNode parseNode)
    {
        return AsyncApiAvroSchemaDeserializer.LoadSchema(parseNode);
    }

    public IEnumerable<string> SupportedFormats => new List<string>
    {
        "application/vnd.apache.avro",
        "application/vnd.apache.avro+json",
        "application/vnd.apache.avro+yaml",
        "application/vnd.apache.avro+json;version=1.9.0",
        "application/vnd.apache.avro+yaml;version=1.9.0",
    };
}