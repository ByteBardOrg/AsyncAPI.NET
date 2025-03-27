namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV2Deserializer
    {
        internal static AsyncApiBindings<IServerBinding> LoadServerBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("serverBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IServerBinding>(pointer);
            }

            var serverBindings = new AsyncApiBindings<IServerBinding>();
            foreach (var property in mapNode)
            {
                var serverBinding = LoadServerBinding(property);

                if (serverBinding != null)
                {
                    serverBindings.Add(serverBinding);
                }
                else
                {
                    mapNode.Context.Diagnostic.Errors.Add(
                        new AsyncApiError(node.Context.GetLocation(), $"ServerBinding {property.Name} is not found"));
                }
            }

            return serverBindings;
        }

        internal static IServerBinding LoadServerBinding(ParseNode node)
        {
            var property = node as PropertyNode;
            try
            {
                if (node.Context.ServerBindingParsers.TryGetValue(property.Name, out var parser))
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
