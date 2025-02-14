// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiChannel> ChannelFixedFields = new()
        {
            { "address", (a, n) => { a.Address = n.GetScalarValue(); } },
            { "messages", (a, n) => { a.Messages = n.CreateMap(LoadMessage); } },
            { "title", (a, n) => { a.Title = n.GetScalarValue(); } },
            { "summary", (a, n) => { a.Summary = n.GetScalarValue(); } },
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "servers", (a, n) => { a.Servers = n.CreateList(LoadServerReference); } },
            { "parameters", (a, n) => { a.Parameters = n.CreateMap(LoadParameter); } },
            { "tags", (a, n) => { a.Tags = n.CreateList(LoadTag); } },
            { "externalDocs", (a, n) => { a.ExternalDocs = LoadExternalDocs(n); } },
            { "bindings", (a, n) => { a.Bindings = LoadChannelBindings(n); } },
        };

        private static readonly PatternFieldMap<AsyncApiChannel> ChannelPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiChannelReference LoadChannelReference(ParseNode node)
        {
            var mapNode = node.CheckMapNode("channel");
            var pointer = mapNode.GetReferencePointer();
            return new AsyncApiChannelReference(pointer);
        }

        public static AsyncApiChannel LoadChannel(ParseNode node)
        {
            var mapNode = node.CheckMapNode("channel");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiChannelReference(pointer);
            }

            var channel = new AsyncApiChannel();

            ParseMap(mapNode, channel, ChannelFixedFields, ChannelPatternFields);

            return channel;
        }
    }
}
