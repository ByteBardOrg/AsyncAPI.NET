// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiOperation> operationFixedFields =
            new()
            {
                {
                    "action", (a, n) => { a.Action = n.GetScalarValue().GetEnumFromDisplayName<AsyncApiAction>(); }
                },
                {
                    "channel", (a, n) => { a.Channel = LoadChannelReference(n); }
                },
                {
                    "title", (a, n) => { a.Title = n.GetScalarValue(); }
                },
                {
                    "summary", (a, n) => { a.Summary = n.GetScalarValue(); }
                },
                {
                    "description", (a, n) => { a.Description = n.GetScalarValue(); }
                },
                {
                    "security", (a, n) => { a.Security = n.CreateList(LoadSecurityScheme); }
                },
                {
                    "tags", (a, n) => a.Tags = n.CreateList(LoadTag)
                },
                {
                    "externalDocs", (a, n) => { a.ExternalDocs = LoadExternalDocs(n); }
                },
                {
                    "bindings", (a, n) => { a.Bindings = LoadOperationBindings(n); }
                },
                {
                    "traits", (a, n) => { a.Traits = n.CreateList(LoadOperationTrait); }
                },
                {
                    "messages", (a, n) => { a.Messages = n.CreateList(LoadMessageReference); }
                },
                {
                    "reply", (a, n) => { a.Reply = LoadOperationReply(n); }
                },
            };

        private static readonly PatternFieldMap<AsyncApiOperation> operationPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

        internal static AsyncApiOperation LoadOperation(ParseNode node)
        {
            var mapNode = node.CheckMapNode("operation");

            var operation = new AsyncApiOperation();

            ParseMap(mapNode, operation, operationFixedFields, operationPatternFields);

            return operation;
        }
    }
}