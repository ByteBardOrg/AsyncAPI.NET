namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    /// <summary>
    /// Class containing logic to deserialize AsyncApi document into
    /// runtime AsyncApi object model.
    /// </summary>
    internal static partial class AsyncApiV2Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiServerVariable> serverVariableFixedFields =
            new()
            {
                {
                    "enum", (a, n) => { a.Enum = n.CreateSimpleList(s => s.GetScalarValue()); }
                },
                {
                    "default", (a, n) => { a.Default = n.GetScalarValue(); }
                },
                {
                    "description", (a, n) => { a.Description = n.GetScalarValue(); }
                },
                {
                    "examples", (a, n) => { a.Examples = n.CreateSimpleList(s => s.GetScalarValue()); }
                },
            };

        private static readonly PatternFieldMap<AsyncApiServerVariable> serverVariablePatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiServerVariable LoadServerVariable(ParseNode node)
        {
            var mapNode = node.CheckMapNode("serverVariable");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiServerVariableReference(pointer);
            }

            var serverVariable = new AsyncApiServerVariable();

            ParseMap(mapNode, serverVariable, serverVariableFixedFields, serverVariablePatternFields);

            return serverVariable;
        }
    }
}
