// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;
    using System.Linq;

    internal static partial class AsyncApiV2Deserializer
    {
        private static FixedFieldMap<AsyncApiParameter> parameterFixedFields = new()
        {
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "schema", (a, n) => { LoadParameterFromSchema(a, n); } },
            { "location", (a, n) => { a.Location = n.GetScalarValue(); } },
        };

        private static PatternFieldMap<AsyncApiParameter> parameterPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        private static void LoadParameterFromSchema(AsyncApiParameter instance, ParseNode node)
        {
            var schema = AsyncApiSchemaDeserializer.LoadSchema(node);
            if (schema.Enum.Any())
            {
                instance.Enum = schema.Enum.Select(e => e.GetValue<string>()).ToList();
            }

            if (schema.Default != null)
            {
                instance.Default = schema.Default.GetValue<string>();
            }

            if (schema.Examples.Any())
            {
                instance.Examples = schema.Examples.Select(e => e.GetValue<string>()).ToList();
            }
        }

        public static AsyncApiParameter LoadParameter(ParseNode node)
        {
            var mapNode = node.CheckMapNode("parameter");

            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiParameterReference(pointer);
            }

            var parameter = new AsyncApiParameter();

            ParseMap(mapNode, parameter, parameterFixedFields, parameterPatternFields);

            return parameter;
        }
    }
}