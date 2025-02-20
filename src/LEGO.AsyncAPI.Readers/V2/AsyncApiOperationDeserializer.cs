// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using System.Threading;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV2Deserializer
    {
        private static volatile int messageCounter = 0;
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
                    "message", (a, n) => { LoadMessages(n, a); }
                },
            };

        private static KeyValuePair<string, AsyncApiMessage> GetMessage(ParseNode node)
        {
            var messageNode = node.CheckMapNode("message");
            string key = string.Empty;
            var message = LoadMessage(node);
            if (message is AsyncApiMessageReference reference)
            {
                key = reference.Reference.Reference.Split("/")[^1];
            }
            else if (messageNode["messageId"] != null)
            {
                key = messageNode["messageId"]?.Value.GetScalarValue();
            }
            else
            {
                key = "anonymous-message-" + Interlocked.Increment(ref messageCounter).ToString();
            }

            return new KeyValuePair<string, AsyncApiMessage>(key, message);
        }

        private static void LoadMessages(ParseNode n, AsyncApiOperation instance)
        {
            var mapNode = n.CheckMapNode("message");
            var messages = new Dictionary<string, AsyncApiMessage>();
            if (mapNode["oneOf"] != null)
            {
                foreach (var node in (ListNode)mapNode["oneOf"].Value)
                {
                    var kvp = GetMessage(node);
                    messages.Add(kvp.Key, kvp.Value);
                }
            }
            else
            {
                var kvp = GetMessage(n);
                messages.Add(kvp.Key, kvp.Value);
            }

            var componentMessageReferences = new Dictionary<string, AsyncApiMessageReference>();
            var componentMessages = new Dictionary<string, AsyncApiMessage>();
            foreach (var message in messages)
            {
                if (message.Value is not AsyncApiMessageReference messageReference)
                {
                    var componentReference = "#/components/messages/" + message.Key;
                    componentMessages.Add(message.Key, message.Value);
                    componentMessageReferences.Add(message.Key, new AsyncApiMessageReference(componentReference));
                    continue;
                }

                componentMessageReferences.Add(message.Key, messageReference);
            }

            n.Context.SetTempStorage(TempStorageKeys.ComponentMessages, componentMessages);
            n.Context.SetTempStorage(TempStorageKeys.OperationMessageReferences, componentMessageReferences, instance);
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