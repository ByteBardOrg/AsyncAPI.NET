namespace ByteBard.AsyncAPI.Readers
{
    using System;
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiExternalDocumentation> externalDocumentationFixedFields = new()
        {
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "url", (a, n) => { a.Url = new Uri(n.GetScalarValue()); } },
        };

        private static PatternFieldMap<AsyncApiExternalDocumentation> externalDocumentationPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiExternalDocumentation LoadExternalDocs(ParseNode node)
        {
            var mapNode = node.CheckMapNode("externalDocs");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiExternalDocumentationReference(pointer);
            }

            var components = new AsyncApiExternalDocumentation();

            ParseMap(mapNode, components, externalDocumentationFixedFields, externalDocumentationPatternFields);

            return components;
        }
    }
}
