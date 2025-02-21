// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;
    using System.Collections.Generic;
    using System.Threading;

    internal static partial class AsyncApiV2Deserializer
    {
        private static readonly FixedFieldMap<AsyncApiChannel> ChannelFixedFields = new()
        {
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "servers", (a, n) => { a.Servers = n.CreateSimpleList(s => new AsyncApiServerReference("#/servers/" + s.GetScalarValue())); } },
            { "subscribe", (a, n) => { /* happens after initial reading */ } },
            { "publish", (a, n) => { /* happens after initial reading */ } },
            { "parameters", (a, n) => { a.Parameters = n.CreateMap(LoadParameter); } },
            { "bindings", (a, n) => { a.Bindings = LoadChannelBindings(n); } },
        };

        private static readonly PatternFieldMap<AsyncApiChannel> ChannelPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiChannel LoadChannel(ParseNode node, string channelAddress = null)
        {
            var mapNode = node.CheckMapNode("channel");
            var pointer = mapNode.GetReferencePointer();
            if (pointer != null)
            {
                return new AsyncApiChannelReference(pointer);
            }

            var channel = new AsyncApiChannel();

            ParseMap(mapNode, channel, ChannelFixedFields, ChannelPatternFields);
            if (channelAddress != null)
            {
                channel.Address = channelAddress;
                LoadV2Operation(mapNode["subscribe"]?.Value, channel, AsyncApiAction.Send);
                LoadV2Operation(mapNode["publish"]?.Value, channel, AsyncApiAction.Receive);
            }

            return channel;
        }

        public static string NormalizeChannelKey(string channelKey)
        {
            string newKey = string.Empty;
            foreach (var character in channelKey)
            {
                if (char.IsLetterOrDigit(character))
                {
                    newKey += character;
                }
            }

            return newKey;
        }

        private static void LoadV2Operation(ParseNode node, AsyncApiChannel instance, AsyncApiAction action)
        {
            if (node == null)
            {
                return;
            }

            var operation = LoadOperation(node);
            var operationKey = node.CheckMapNode("operation")?["operationId"]?.Value.GetScalarValue() ?? "anonymous-operation-" + Interlocked.Increment(ref node.Context.OperationCounter).ToString();
            operation.Action = action;
            operation.Channel = new AsyncApiChannelReference("#/channels/" + NormalizeChannelKey(instance.Address));

            var globalOperations = node.Context.GetFromTempStorage<Dictionary<string, AsyncApiOperation>>(TempStorageKeys.Operations);
            globalOperations ??= new Dictionary<string, AsyncApiOperation>();
            globalOperations.Add(operationKey, operation);
            node.Context.SetTempStorage(TempStorageKeys.Operations, globalOperations);
        }
    }
}
