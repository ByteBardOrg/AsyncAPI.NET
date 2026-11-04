namespace ByteBard.AsyncAPI.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ByteBard.AsyncAPI.Bindings;
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.Interface;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public class BindingDeserializer
    {
        public static T LoadBinding<T>(string nodeName, ParseNode node, FixedFieldMap<T> fieldMap)
   where T : IBinding, new()
        {
            var mapNode = node.CheckMapNode(nodeName);
            var binding = new T();

            AsyncApiV2Deserializer.ParseMap(mapNode, binding, fieldMap, BindingPatternExtensionFields<T>());

            return binding;
        }

        internal static AsyncApiBindings<TBinding> LoadBindings<TBinding>(
            MapNode mapNode,
            IDictionary<string, IBindingParser<TBinding>> parsers,
            PatternFieldMap<AsyncApiBindings<TBinding>> patternFields,
            AsyncApiVersion version = AsyncApiVersion.AsyncApi2_0)
            where TBinding : IBinding
        {
            var context = mapNode.Context;
            var bindings = new AsyncApiBindings<TBinding>();

            // TBinding is the binding interface (e.g. IChannelBinding), so drop the leading "I" for the diagnostics.
            var bindingType = typeof(TBinding).Name.Substring(1);
            foreach (var property in mapNode)
            {
                var key = property.Name;
                if (version == AsyncApiVersion.AsyncApi3_0 && TryGetCustomBindingKey(property.Name, parsers, out var customKey))
                {
                    // AsyncAPI 3 writes a custom binding as an extension (x-<key>); read it back as the binding.
                    key = customKey;
                }
                else if (patternFields.Keys.Any(isMatch => isMatch(property.Name)))
                {
                    property.ParseField(bindings, patternFields);
                    continue;
                }

                if (!parsers.TryGetValue(key, out var parser))
                {
                    // Reported as an error because the binding is dropped from the model and cannot be written back.
                    context.Diagnostic.Errors.Add(
                        new AsyncApiError(context.GetLocation(), $"{bindingType} {key} is not found"));
                    continue;
                }

                if (version == AsyncApiVersion.AsyncApi3_0 && key == property.Name && !AsyncApiBindingKeys.IsSpecificationKey(key))
                {
                    // AsyncAPI 3 bindings objects only allow the specification's binding names and extensions. The binding
                    // is still read, and written back as x-<key>.
                    context.Diagnostic.Errors.Add(
                        new AsyncApiError(
                            context.GetLocation(),
                            $"{bindingType} {key} is not an AsyncAPI 3 binding name; write a custom binding as {AsyncApiBindingKeys.ToV3Key(key)}"));
                }

                try
                {
                    var binding = parser.LoadBinding(property);
                    if (binding != null)
                    {
                        bindings.Add(binding);
                    }
                }
                catch (AsyncApiException ex)
                {
                    ex.Pointer = context.GetLocation();
                    context.Diagnostic.Errors.Add(new AsyncApiError(ex));
                }
            }

            return bindings;
        }

        private static bool TryGetCustomBindingKey<TBinding>(
            string propertyName,
            IDictionary<string, IBindingParser<TBinding>> parsers,
            out string key)
            where TBinding : IBinding
        {
            key = null;
            if (!propertyName.StartsWith(AsyncApiConstants.ExtensionFieldNamePrefix, StringComparison.Ordinal))
            {
                return false;
            }

            var candidate = propertyName.Substring(AsyncApiConstants.ExtensionFieldNamePrefix.Length);
            if (AsyncApiBindingKeys.IsSpecificationKey(candidate) || !parsers.ContainsKey(candidate))
            {
                return false;
            }

            key = candidate;
            return true;
        }

        private static PatternFieldMap<T> BindingPatternExtensionFields<T>()
    where T : IBinding, new()
        {
            return new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, AsyncApiV2Deserializer.LoadExtension(p, n)) },
            };
        }
    }
}
