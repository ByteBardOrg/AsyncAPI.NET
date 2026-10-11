namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly PatternFieldMap<AsyncApiBindings<IOperationBinding>> operationBindingPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

        internal static AsyncApiBindings<IOperationBinding> LoadOperationBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("operationBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IOperationBinding>(pointer);
            }

            return BindingDeserializer.LoadBindings("OperationBinding", mapNode, node.Context.OperationBindingParsers, operationBindingPatternFields);
        }
    }
}
