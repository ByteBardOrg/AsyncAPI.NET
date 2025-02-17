// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;
    using System;

    /// <summary>
    /// Class containing logic to deserialize AsyncApi document into
    /// runtime AsyncApi object model.
    /// </summary>
    internal static partial class AsyncApiV2Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiServer> serverFixedFields = new()
        {
            {
                "url", (a, n) => { SetHostAndPathname(a, n); }
            },
            {
                "description", (a, n) => { a.Description = n.GetScalarValue(); }
            },
            {
                "variables", (a, n) => { a.Variables = n.CreateMap(LoadServerVariable); }
            },
            {
                "security", (a, n) => { a.Security = n.CreateList(LoadSecurityRequirement); }
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

        private static void SetHostAndPathname(AsyncApiServer a, ParseNode n)
        {
            var value = n.GetScalarValue();
            if (!value.Contains("://"))
            {
                // Set arbitrary protocol.
                value = "unknown://" + value;
            }

            var uri = new Uri(value);

            a.Host = uri.Host;
            a.PathName = uri.LocalPath;
        }

        private static readonly PatternFieldMap<AsyncApiServer> serverPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiServer LoadServer(ParseNode node)
        {
            var mapNode = node.CheckMapNode("servers");
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
