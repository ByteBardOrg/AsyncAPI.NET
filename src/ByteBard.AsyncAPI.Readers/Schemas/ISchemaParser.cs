namespace ByteBard.AsyncAPI.Readers;

using System.Collections.Generic;
using Models.Interfaces;
using ParseNodes;

public interface IAsyncApiSchemaParser
{
    IAsyncApiSchema LoadSchema(ParseNode node);
    IEnumerable<string> SupportedFormats { get; }
}