namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        internal static AsyncApiBindings<IChannelBinding> LoadChannelBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("channelBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IChannelBinding>(pointer);
            }

            var channelBindings = new AsyncApiBindings<IChannelBinding>();
            foreach (var property in mapNode)
            {
                var channelBinding = LoadChannelBinding(property);

                if (channelBinding != null)
                {
                    channelBindings.Add(channelBinding);
                }
                else
                {
                    mapNode.Context.Diagnostic.Errors.Add(
                        new AsyncApiError(node.Context.GetLocation(), $"ChannelBinding {property.Name} is not found"));
                }
            }

            mapNode.ParseFields(channelBindings, channelBindingPatternFields);
            
            return channelBindings;
        }

        private static readonly PatternFieldMap<AsyncApiBindings<IChannelBinding>> channelBindingPatternFields =
    new()
    {
        { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
    };

        private static IChannelBinding LoadChannelBinding(ParseNode node)
        {
            var property = node as PropertyNode;
            try
            {
                if (node.Context.ChannelBindingParsers.TryGetValue(property.Name, out var parser))
                {
                    return parser.LoadBinding(property);
                }
            }
            catch (AsyncApiException ex)
            {
                ex.Pointer = node.Context.GetLocation();
                node.Context.Diagnostic.Errors.Add(new AsyncApiError(ex));
            }

            return null;
        }
    }
}
