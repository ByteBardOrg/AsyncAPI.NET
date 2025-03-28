namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiOperationTrait> operationTraitFixedFields = new()
        {
            { "title", (a, n) => { a.Title = n.GetScalarValue(); } },
            { "summary", (a, n) => { a.Summary = n.GetScalarValue(); } },
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "tags", (a, n) => { a.Tags = n.CreateList(LoadTag); } },
            { "security", (a, n) => { a.Security = n.CreateList(LoadSecurityScheme); } },
            { "externalDocs", (a, n) => { a.Tags = n.CreateList(LoadTag); } },
            { "bindings", (a, n) => { a.Bindings = LoadOperationBindings(n); } },
        };

        private static PatternFieldMap<AsyncApiOperationTrait> operationTraitPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiOperationTrait LoadOperationTrait(ParseNode node)
        {
            var mapNode = node.CheckMapNode("traits");

            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiOperationTraitReference(pointer);
            }

            var operationTrait = new AsyncApiOperationTrait();

            ParseMap(mapNode, operationTrait, operationTraitFixedFields, operationTraitPatternFields);

            return operationTrait;
        }
    }
}