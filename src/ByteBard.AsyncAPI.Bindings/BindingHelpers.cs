namespace ByteBard.AsyncAPI.Bindings
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public static class BindingHelpers
    {
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
