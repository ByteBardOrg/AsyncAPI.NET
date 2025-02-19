// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Exceptions;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    /// <summary>
    /// Class containing logic to deserialize AsyncApi document into
    /// runtime AsyncApi object model.
    /// </summary>
    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiMessage> messageFixedFields = new()
        {
            {
                "messageId", (a, n) => { }
            },
            {
                "headers", (a, n) => { a.Headers = LoadMultiFormatSchema(n); }
            },
            {
                "payload", (a, n) => { a.Payload = LoadMultiFormatSchema(n); }
            },
            {
                "correlationId", (a, n) => { a.CorrelationId = LoadCorrelationId(n); }
            },
            {
                "schemaFormat", (a, n) => { /* loaded as part of multiformatschema */ }
            },
            {
                "contentType", (a, n) => { a.ContentType = n.GetScalarValue(); }
            },
            {
                "name", (a, n) => { a.Name = n.GetScalarValue(); }
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
                "tags", (a, n) => a.Tags = n.CreateList(LoadTag)
            },
            {
                "externalDocs", (a, n) => { a.ExternalDocs = LoadExternalDocs(n); }
            },
            {
                "bindings", (a, n) => { a.Bindings = LoadMessageBindings(n); }
            },
            {
                "examples", (a, n) => a.Examples = n.CreateList(LoadExample)
            },
            {
                "traits", (a, n) => a.Traits = n.CreateList(LoadMessageTrait)
            },
        };

        private static readonly PatternFieldMap<AsyncApiMessage> messagePatternFields = new()
        {
            { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
        };

        public static AsyncApiMessageReference LoadMessageReference(ParseNode node)
        {
            var mapNode = node.CheckMapNode("message");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiMessageReference(pointer);
            }

            return null;
        }

        public static AsyncApiMessage LoadMessage(ParseNode node)
        {
            var mapNode = node.CheckMapNode("message");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiMessageReference(pointer);
            }

            var message = new AsyncApiMessage();

            ParseMap(mapNode, message, messageFixedFields, messagePatternFields);

            return message;
        }
    }
}