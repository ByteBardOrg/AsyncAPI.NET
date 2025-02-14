// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal static partial class AsyncApiV3Deserializer
    {
        private static FixedFieldMap<AsyncApiComponents> componentsFixedFields = new()
        {
            { "schemas", (a, n) => a.Schemas = n.CreateMap(LoadMultiFormatSchema) },
            { "servers", (a, n) => a.Servers = n.CreateMap(LoadServer) },
            { "channels", (a, n) => a.Channels = n.CreateMap(LoadChannel) },
            { "operations", (a, n) => a.Operations = n.CreateMap(LoadOperation) },
            { "messages", (a, n) => a.Messages = n.CreateMap(LoadMessage) },
            { "securitySchemes", (a, n) => a.SecuritySchemes = n.CreateMap(LoadSecurityScheme) },
            { "serverVariables", (a, n) => a.ServerVariables = n.CreateMap(LoadServerVariable) },
            { "parameters", (a, n) => a.Parameters = n.CreateMap(LoadParameter) },
            { "correlationIds", (a, n) => a.CorrelationIds = n.CreateMap(LoadCorrelationId) },
            { "replies", (a, n) => a.Replies = n.CreateMap(LoadOperationReply) },
            { "replyAddresses", (a, n) => a.ReplyAddresses = n.CreateMap(LoadOperationReplyAddress) },
            { "externalDocs", (a, n) => a.ExternalDocs = n.CreateMap(LoadExternalDocs) },
            { "tags", (a, n) => a.Tags = n.CreateMap(LoadTag) },
            { "operationTraits", (a, n) => a.OperationTraits = n.CreateMap(LoadOperationTrait) },
            { "messageTraits", (a, n) => a.MessageTraits = n.CreateMap(LoadMessageTrait) },
            { "serverBindings", (a, n) => a.ServerBindings = n.CreateMap(LoadServerBindings) },
            { "channelBindings", (a, n) => a.ChannelBindings = n.CreateMap(LoadChannelBindings) },
            { "operationBindings", (a, n) => a.OperationBindings = n.CreateMap(LoadOperationBindings) },
            { "messageBindings", (a, n) => a.MessageBindings = n.CreateMap(LoadMessageBindings) },
        };

        private static PatternFieldMap<AsyncApiComponents> componentsPatternFields =
            new()
            {
                { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
            };

        public static AsyncApiComponents LoadComponents(ParseNode node)
        {
            var mapNode = node.CheckMapNode("components");
            var components = new AsyncApiComponents();

            ParseMap(mapNode, components, componentsFixedFields, componentsPatternFields);

            return components;
        }
    }
}
