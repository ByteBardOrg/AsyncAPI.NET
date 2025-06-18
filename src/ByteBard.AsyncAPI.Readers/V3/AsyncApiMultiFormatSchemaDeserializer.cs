namespace ByteBard.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using System.Linq;
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        public static AsyncApiMultiFormatSchema LoadMultiFormatSchema(ParseNode node)
        {
            var mapNode = node.CheckMapNode("MultiFormatSchema");
            var pointer = mapNode.GetReferencePointer();

            var schemaFormat = new AsyncApiMultiFormatSchema();
            var defaultSchemaFormat = "application/vnd.aai.asyncapi+json;version=3.0.0";
            if (pointer != null)
            {
                return new AsyncApiMultiFormatSchemaReference(pointer);
            }

            // Not a pointer and no schemaFormat means it MUST be a jsonSchema,
            if (mapNode["schemaFormat"] == null)
            {
                schemaFormat.Schema = LoadSchema(node, defaultSchemaFormat);
                schemaFormat.SchemaFormat = defaultSchemaFormat;
                return schemaFormat;
            }

            var format = mapNode["schemaFormat"].Value.GetScalarValue();
            var schema = mapNode["schema"].Value;
            schemaFormat.Schema = LoadSchema(schema, format);
            schemaFormat.SchemaFormat = format;
            return schemaFormat;
        }
        
        private static IAsyncApiSchema LoadSchema(ParseNode n, string format)
        {
            if (n == null)
            {
                return null;
            }

            var registry = n.Context.SchemaParserRegistry;
            var parser = registry.GetParser(format);
            if (parser != null)
            {
                return parser.LoadSchema(n);
            }

            var supportedFormats = registry.GetSupportedFormats();
            throw new AsyncApiException($"Could not deserialize Schema. Supported formats are {string.Join(", ", supportedFormats)}");
        }
    }
}