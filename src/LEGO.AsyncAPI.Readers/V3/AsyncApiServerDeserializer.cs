// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    /// <summary>
    /// Class containing logic to deserialize AsyncApi document into
    /// runtime AsyncApi object model.
    /// </summary>
    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiServer> serverFixedFields = new()
        {
            {
                "host", (a, n) => { a.Host = n.GetScalarValue(); }
            },
            {
                "pathname", (a, n) => { a.PathName = n.GetScalarValue(); }
            },
            {
                "title", (a, n) => { a.Title = n.GetScalarValue(); }
            },
            {
                "summary", (a, n) => { a.Summary = n.GetScalarValue(); }
            },
            {
                "externalDocs", (a, n) => { a.ExternalDocs = LoadExternalDocs(n); }
            },
            {
                "description", (a, n) => { a.Description = n.GetScalarValue(); }
            },
            {
                "variables", (a, n) => { a.Variables = n.CreateMap(LoadServerVariable); }
            },
            {
                "security", (a, n) => { a.Security = n.CreateList(LoadSecurityScheme); }
            },
            {
                "tags", (a, n) => { a.Tags = n.CreateList(LoadTag); }
            },
            {
                "bindings", (o, n) => { o.Bindings = LoadServerBindings(n); }
            },
            {
                "protocolVersion", (a, n) => { a.ProtocolVersion = n.GetScalarValue(); }
            },
            {
                "protocol", (a, n) => { a.Protocol = n.GetScalarValue(); }
            },
        };

        private static readonly PatternFieldMap<AsyncApiServer> serverPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiServerReference LoadServerReference(ParseNode node)
        {
            var mapNode = node.CheckMapNode("server");
            var pointer = mapNode.GetReferencePointer();
            return new AsyncApiServerReference(pointer);
        }

        public static AsyncApiServer LoadServer(ParseNode node)
        {
            var mapNode = node.CheckMapNode("server");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiServerReference(pointer);
            }

            var server = new AsyncApiServer();

            ParseMap(mapNode, server, serverFixedFields, serverPatternFields);

            return server;
        }
    }
}
