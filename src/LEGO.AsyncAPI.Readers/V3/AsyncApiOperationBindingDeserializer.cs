// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Exceptions;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        internal static AsyncApiBindings<IOperationBinding> LoadOperationBindings(ParseNode node)
        {
            var mapNode = node.CheckMapNode("operationBindings");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiBindingsReference<IOperationBinding>(pointer);
            }

            var operationBindings = new AsyncApiBindings<IOperationBinding>();
            foreach (var property in mapNode)
            {
                var operationBinding = LoadOperationBinding(property);

                if (operationBinding != null)
                {
                    operationBindings.Add(operationBinding);
                }
                else
                {
                    mapNode.Context.Diagnostic.Errors.Add(
                        new AsyncApiError(node.Context.GetLocation(), $"OperationBinding {property.Name} is not found"));
                }
            }

            // #ToFix Write test to show that we can still deserialize bindings correctly and still have extensions on the parent.
            mapNode.ParseFields(operationBindings, null, operationBindingPatternFields);
            return operationBindings;
        }

        private static readonly PatternFieldMap<AsyncApiBindings<IOperationBinding>> operationBindingPatternFields =
    new()
    {
        { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
    };

        internal static IOperationBinding LoadOperationBinding(ParseNode node)
        {
            var property = node as PropertyNode;
            try
            {
                if (node.Context.OperationBindingParsers.TryGetValue(property.Name, out var parser))
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
