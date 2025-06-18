namespace ByteBard.AsyncAPI.Readers;

using System.Collections.Generic;
using System.Linq;

public class SchemaParserRegistry
{
    public SchemaParserRegistry()
    {
        this.RegisterParser(new JsonSchemaParser());
        this.RegisterParser(new AvroSchemaParser());
    }
    
    private readonly Dictionary<string, IAsyncApiSchemaParser> parsers = new();
    private readonly Dictionary<string, string> formatToPrefix = new();

    public void RegisterParser(IAsyncApiSchemaParser deserializer)
    {
        foreach (var format in deserializer.SupportedFormats)
        {
            this.parsers[format] = deserializer;
            this.formatToPrefix[format] = format;
        }
    }

    public IAsyncApiSchemaParser GetParser(string format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return this.parsers.Values.FirstOrDefault(d => d is JsonSchemaParser);
        }

        if (this.parsers.TryGetValue(format, out var parser))
        {
            return parser;
        }

        var matchingPrefix = this.formatToPrefix.Keys.FirstOrDefault(prefix => format.StartsWith(prefix));
        if (matchingPrefix != null)
        {
            return this.parsers[matchingPrefix];
        }

        return null;
    }

    public IEnumerable<string> GetSupportedFormats()
    {
        return this.parsers.Keys;
    }
}