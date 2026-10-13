namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly PatternFieldMap<AsyncApiBindings<IChannelBinding>> channelBindingPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

        internal static AsyncApiBindings<IChannelBinding> LoadChannelBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("channelBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IChannelBinding>(pointer);
            }

            return BindingDeserializer.LoadBindings(mapNode, node.Context.ChannelBindingParsers, channelBindingPatternFields);
        }
    }
}
