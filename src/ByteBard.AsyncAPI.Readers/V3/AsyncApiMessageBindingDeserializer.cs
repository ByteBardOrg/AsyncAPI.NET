namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly PatternFieldMap<AsyncApiBindings<IMessageBinding>> messageBindingPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

        internal static AsyncApiBindings<IMessageBinding> LoadMessageBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("messageBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IMessageBinding>(pointer);
            }

            return BindingDeserializer.LoadBindings("MessageBinding", mapNode, node.Context.MessageBindingParsers, messageBindingPatternFields);
        }
    }
}
