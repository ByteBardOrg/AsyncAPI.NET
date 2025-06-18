namespace ByteBard.AsyncAPI.Readers;

using System.Collections.Generic;
using Models.Interfaces;
using ParseNodes;

public class JsonSchemaParser: IAsyncApiSchemaParser
{
    public IAsyncApiSchema LoadSchema(ParseNode node)
    {
        return AsyncApiJsonSchemaDeserializer.LoadSchema(node);
    }

    public IEnumerable<string> SupportedFormats => new List<string>
    {
        "application/vnd.aai.asyncapi",
        "application/vnd.aai.asyncapi+json",
        "application/vnd.aai.asyncapi+yaml",
        "application/schema+json;version=draft-07",
        "application/schema+yaml;version=draft-07",
    };
}