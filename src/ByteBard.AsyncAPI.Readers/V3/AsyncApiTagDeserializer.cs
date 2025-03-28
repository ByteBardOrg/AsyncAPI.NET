namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiTag> tagsFixedFields = new()
        {
            { "name", (a, n) => { a.Name = n.GetScalarValue(); } },
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "externalDocs", (a, n) => { a.ExternalDocs = LoadExternalDocs(n); } },
        };

        private static PatternFieldMap<AsyncApiTag> tagsPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiTag LoadTag(ParseNode node)
        {
            var mapNode = node.CheckMapNode("tags");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiTagReference(pointer);
            }

            var tag = new AsyncApiTag();

            ParseMap(mapNode, tag, tagsFixedFields, tagsPatternFields);

            return tag;
        }
    }
}
