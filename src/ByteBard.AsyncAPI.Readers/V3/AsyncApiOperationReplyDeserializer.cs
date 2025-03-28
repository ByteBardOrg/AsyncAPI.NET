namespace ByteBard.AsyncAPI.Readers
{
    using ByteBard.AsyncAPI.Extensions;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiOperationReply> replyFixedFields = new()
    {
        { "address", (a, n) => { a.Address = LoadOperationReplyAddress(n); } },
        { "channel", (a, n) => { a.Channel = LoadChannelReference(n); } },
        { "messages", (a, n) => { a.Messages = n.CreateList(LoadMessageReference); } },
    };

        private static PatternFieldMap<AsyncApiOperationReply> replyPatternFields =
            new()
            {
        { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiOperationReply LoadOperationReply(ParseNode node)
        {
            var mapNode = node.CheckMapNode("reply");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiOperationReplyReference(pointer);
            }

            var reply = new AsyncApiOperationReply();

            ParseMap(mapNode, reply, replyFixedFields, replyPatternFields);

            return reply;
        }
    }
}