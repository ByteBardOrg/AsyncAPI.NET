namespace ByteBard.AsyncAPI.Bindings
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public static class BindingHelpers
    {
        /// <summary>
        /// Parses a nested object using the supplied field handlers.
        /// </summary>
        /// <param name="node">The nested object to parse.</param>
        /// <param name="fixedFieldMap">Handlers for recognized property names; other properties follow the reader's unmapped-member setting.</param>
        /// <returns>A new instance populated from recognized fields, which may be incomplete when field parsing reports errors.</returns>
        /// <remarks>Field-level reader and AsyncAPI exceptions are recorded as diagnostics; other handler exceptions propagate.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException"><paramref name="node"/> is not a map.</exception>
        public static T ParseMap<T>(this ParseNode node, FixedFieldMap<T> fixedFieldMap)
            where T : new()
        {
            var mapNode = node.CheckMapNode(node.Context.GetLocation());
            if (mapNode == null)
            {
                return default(T);
            }

            var instance = new T();
            var emptyPatternFields = new Dictionary<Func<string, bool>, Action<T, string, ParseNode>>();

            foreach (var propertyNode in mapNode)
            {
                propertyNode.ParseField(instance, fixedFieldMap, emptyPatternFields);
            }

            return instance;
        }

        public static T ParseMapWithExtensions<T>(this ParseNode node, FixedFieldMap<T> fixedFieldMap)
            where T : IAsyncApiExtensible, new()
        {
            var mapNode = node.CheckMapNode(node.Context.GetLocation());
            if (mapNode == null)
            {
                return default(T);
            }

            var instance = new T();

            foreach (var propertyNode in mapNode)
            {
                propertyNode.ParseField(instance, fixedFieldMap, ExtensionHelpers.GetExtensionsFieldMap<T>());
            }

            return instance;
        }
    }
}
