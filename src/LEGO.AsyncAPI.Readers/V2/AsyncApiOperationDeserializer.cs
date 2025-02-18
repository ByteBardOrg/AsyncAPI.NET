// Copyright (c) The LEGO Group. All rights reserved.

using System.Linq;

namespace LEGO.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV2Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiOperation> operationFixedFields =
            new()
            {
                {
                    "operationId", (a, n) => { }
                },
                {
                    "summary", (a, n) => { a.Summary = n.GetScalarValue(); }
                },
                {
                    "description", (a, n) => { a.Description = n.GetScalarValue(); }
                },
                {
                    "security", (a, n) => { a.Security = n.CreateList(LoadSecurityRequirement); }
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
                    "message", (a, n) => { a.Messages = LoadMessages(n); }
                },
            };

        private static IList<AsyncApiMessageReference> LoadMessages(ParseNode n)
        {
            var mapNode = n.CheckMapNode("message");
            List<AsyncApiMessage> messages;
            if (mapNode["oneOf"] != null)
            {
                messages = mapNode["oneOf"].Value.CreateList(LoadMessage);
            }

            messages = new List<AsyncApiMessage> { LoadMessage(n) };
            var messageReferences = new List<AsyncApiMessageReference>();
            var counter = 0;

            foreach (var message in messages)
            {
                if (message is not AsyncApiMessageReference messageReference)
                {
                    var reference = "#/components/messages/upgradedOperationMessage_" + (mapNode["operationId"]?.GetScalarValue() + message.Name) ?? counter.ToString();

                    n.Context.Workspace.RegisterComponent(reference, message);
                    messageReferences.Add(new AsyncApiMessageReference(reference));
                    counter++;
                    continue;
                }

                messageReferences.Add(messageReference);
            }

            return messageReferences;
        }

        private static readonly PatternFieldMap<AsyncApiOperation> operationPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (o, p, n) => o.AddExtension(p, LoadExtension(p, n)) },
            };

        internal static AsyncApiOperation LoadOperation(ParseNode node)
        {
            var mapNode = node.CheckMapNode("operation");
            if (mapNode == null)
            {
                return null;
            }
            var operation = new AsyncApiOperation();

            ParseMap(mapNode, operation, operationFixedFields, operationPatternFields);

            return operation;
        }
    }
}