namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV2Deserializer
    {
        private static readonly PatternFieldMap<AsyncApiBindings<IServerBinding>> serverBindingPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

        internal static AsyncApiBindings<IServerBinding> LoadServerBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("serverBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IServerBinding>(pointer);
            }

            return BindingDeserializer.LoadBindings(mapNode, node.Context.ServerBindingParsers, serverBindingPatternFields);
        }
    }
}
