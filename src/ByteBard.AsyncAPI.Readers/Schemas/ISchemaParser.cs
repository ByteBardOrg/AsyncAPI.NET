namespace ByteBard.AsyncAPI.Readers;

using System.Collections.Generic;
using Models.Interfaces;
using ParseNodes;

public interface ISchemaParser
{
    IAsyncApiSchema LoadSchema(ParseNode node);
    IEnumerable<string> SupportedFormats { get; }
}