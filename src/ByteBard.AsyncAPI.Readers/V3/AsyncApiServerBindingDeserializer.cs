namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Extensions;

    internal static partial class AsyncApiV3Deserializer
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

            mapNode.ParseFields(serverBindings, serverBindingPatternFields);
            return serverBindings;
        }

        private static readonly PatternFieldMap<AsyncApiBindings<IServerBinding>> serverBindingPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

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
