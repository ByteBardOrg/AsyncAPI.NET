// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LEGO.AsyncAPI.Exceptions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV2Deserializer
    {
        public static AsyncApiMultiFormatSchema LoadMultiFormatSchema(ParseNode node)
        {
            var mapNode = node.CheckMapNode("MultiFormatSchema");
            var pointer = mapNode.GetReferencePointer();

            var schemaFormat = new AsyncApiMultiFormatSchema();
            var defaultSchemaFormat = "application/vnd.aai.asyncapi+json;version=3.0.0";
            if (pointer != null)
            {
                schemaFormat.Schema = new AsyncApiJsonSchemaReference(pointer);
                schemaFormat.SchemaFormat = defaultSchemaFormat;
                return schemaFormat;
            }

            // Not a pointer and no schemaFormat means it MUST be a jsonSchema,
            if (mapNode["schemaFormat"] == null)
            {
                schemaFormat.Schema = LoadSchema(node, defaultSchemaFormat);
                schemaFormat.SchemaFormat = defaultSchemaFormat;
                return schemaFormat;
            }

            var format = mapNode["schemaFormat"].Value.GetScalarValue();
            schemaFormat.Schema = LoadSchema(node, LoadSchemaFormat(format));
            schemaFormat.SchemaFormat = format;
            return schemaFormat;

        }

        private static IAsyncApiSchema LoadSchema(ParseNode n, string format)
        {

            if (n == null)
            {
                return null;
            }

            switch (format)
            {
                case null:
                case "":
                case var _ when SupportedJsonSchemaFormats.Where(s => format.StartsWith(s)).Any():
                    return AsyncApiSchemaDeserializer.LoadSchema(n);
                case var _ when SupportedAvroSchemaFormats.Where(s => format.StartsWith(s)).Any():
                    return AsyncApiAvroSchemaDeserializer.LoadSchema(n);
                default:
                    var supportedFormats = SupportedJsonSchemaFormats.Concat(SupportedAvroSchemaFormats);
                    throw new AsyncApiException($"'Could not deserialize Schema. Supported formats are {string.Join(", ", supportedFormats)}");
            }
        }

        static readonly IEnumerable<string> SupportedJsonSchemaFormats = new List<string>
        {
            "application/vnd.aai.asyncapi+json",
            "application/vnd.aai.asyncapi+yaml",
            "application/vnd.aai.asyncapi",
            "application/schema+json;version=draft-07",
            "application/schema+yaml;version=draft-07",
        };

        static readonly IEnumerable<string> SupportedAvroSchemaFormats = new List<string>
        {
            "application/vnd.apache.avro",
            "application/vnd.apache.avro+json",
            "application/vnd.apache.avro+yaml",
            "application/vnd.apache.avro+json;version=1.9.0",
            "application/vnd.apache.avro+yaml;version=1.9.0",
        };

        private static string LoadSchemaFormat(string schemaFormat)
        {
            var supportedFormats = SupportedJsonSchemaFormats.Concat(SupportedAvroSchemaFormats);
            if (!supportedFormats.Where(s => schemaFormat.StartsWith(s)).Any())
            {
                throw new AsyncApiException($"'{schemaFormat}' is not a supported format. Supported formats are {string.Join(", ", supportedFormats)}");
            }

            return schemaFormat;
        }
    }
}