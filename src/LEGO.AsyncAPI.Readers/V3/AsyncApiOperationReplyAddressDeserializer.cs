// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiOperationReplyAddress> replyAddressFixedFields = new()
    {
        { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
        { "location", (a, n) => { a.Location = n.GetScalarValue(); } },
    };

        private static PatternFieldMap<AsyncApiOperationReplyAddress> replyAddressPatternFields =
            new()
            {
        { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiOperationReplyAddress LoadOperationReplyAddress(ParseNode node)
        {
            var mapNode = node.CheckMapNode("address");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiOperationReplyAddressReference(pointer);
            }

            var reply = new AsyncApiOperationReplyAddress();

            ParseMap(mapNode, reply, replyAddressFixedFields, replyAddressPatternFields);

            return reply;
        }
    }
}