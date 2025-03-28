namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public static class ExtensionHelpers
    {
        public static PatternFieldMap<T> GetExtensionsFieldMap<T>()
            where T : IAsyncApiExtensible
        {
            return new()
            {
                {
                    s => s.StartsWith("x-"),
                    (a, p, n) => a.AddExtension(p, LoadExtension(p, n))
                },
            };
        }

        public static IAsyncApiExtension LoadExtension(string name, ParseNode node)
        {
            try
            {
                if (node.Context.ExtensionParsers.TryGetValue(name, out var parser))
                {
                    return parser(node.CreateAny());
                }
            }
            catch (AsyncApiException ex)
            {
                ex.Pointer = node.Context.GetLocation();
                node.Context.Diagnostic.Errors.Add(new AsyncApiError(ex));
            }

            return node.CreateAny();
        }
    }
}
