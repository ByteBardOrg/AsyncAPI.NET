namespace ByteBard.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using System.Linq;
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
            PatternFieldMap<AsyncApiBindings<TBinding>> patternFields)
            where TBinding : IBinding
        {
            var context = mapNode.Context;

            // TBinding is the binding interface (e.g. IChannelBinding), so drop the leading "I" for the diagnostic.
            var bindingType = typeof(TBinding).Name.Substring(1);
            var bindings = new AsyncApiBindings<TBinding>();
            foreach (var property in mapNode)
            {
                if (patternFields.Keys.Any(isMatch => isMatch(property.Name)))
                {
                    continue;
                }

                if (!parsers.TryGetValue(property.Name, out var parser))
                {
                    // Reported as an error because the binding is dropped from the model and cannot be written back.
                    context.Diagnostic.Errors.Add(
                        new AsyncApiError(context.GetLocation(), $"{bindingType} {property.Name} is not found"));
                    continue;
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

            mapNode.ParseFields(bindings, patternFields);

            return bindings;
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
