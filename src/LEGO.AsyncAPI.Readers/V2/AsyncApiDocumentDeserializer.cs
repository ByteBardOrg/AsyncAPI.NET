// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;
    using System.Collections.Generic;
    using System.Linq;

    internal static partial class AsyncApiV2Deserializer
    {
        private static FixedFieldMap<AsyncApiDocument> asyncApiFixedFields = new()
        {
            { "asyncapi", (a, n) => { a.Asyncapi = "2.6.0"; } },
            { "id", (a, n) => a.Id = n.GetScalarValue() },
            { "info", (a, n) => a.Info = LoadInfo(n) },
            { "components", (a, n) => a.Components = LoadComponents(n) }, // Load before anything else so upgrading can go smoothly.
            { "servers", (a, n) => a.Servers = n.CreateMap(LoadServer) },
            { "defaultContentType", (a, n) => a.DefaultContentType = n.GetScalarValue() },
            { "channels", (a, n) => a.Channels = n.CreateMap(key => NormalizeChannelKey(key), (n2, originalKey) => LoadChannel(n2, channelAddress: originalKey)) },
            { "tags", (a, n) => a.Info.Tags = n.CreateList(LoadTag) },
            { "externalDocs", (a, n) => a.Info.ExternalDocs = LoadExternalDocs(n) },
        };

        private static PatternFieldMap<AsyncApiDocument> asyncApiPatternFields = new()
        {
            { s => s.StartsWith("x-"), (a, p, n) => a.AddExtension(p, LoadExtension(p, n)) },
        };

        private static void SetSecuritySchemeScopes(ParsingContext context, AsyncApiDocument document)
        {
            if (document.Components?.SecuritySchemes == null)
            { return; }
            foreach (var securityScheme in document.Components?.SecuritySchemes)
            {
                var scopes = context.GetFromTempStorage<List<string>>(TempStorageKeys.SecuritySchemeScopes, securityScheme.Key);
                if (scopes == null)
                {
                    return;
                }

                foreach (var scope in scopes)
                {
                    securityScheme.Value.Scopes.Add(scope);
                }
            }
        }

        public static AsyncApiDocument LoadAsyncApi(RootNode rootNode)
        {
            var document = new AsyncApiDocument();

            var asyncApiNode = rootNode.GetMap();
            ParseMap(asyncApiNode, document, asyncApiFixedFields, asyncApiPatternFields);

            SetSecuritySchemeScopes(asyncApiNode.Context, document);
            SetMessages(asyncApiNode.Context, document);
            SetOperations(asyncApiNode.Context, document);
            return document;
        }

        private static void SetMessages(ParsingContext context, AsyncApiDocument document)
        {
            var messages = context.GetFromTempStorage<Dictionary<string, AsyncApiMessage>>(TempStorageKeys.ComponentMessages);
            if (messages == null)
            {
                return;
            }
            foreach (var message in messages)
            {
                document?.Components?.Messages.Add(message.Key, message.Value);
            }
        }

        private static void SetOperations(ParsingContext context, AsyncApiDocument document)
        {
            var operations = context.GetFromTempStorage<Dictionary<string, AsyncApiOperation>>(TempStorageKeys.Operations);
            if (operations == null)
            {
                return;
            }
            foreach (var operation in operations)
            {
                document.Operations.Add(operation);
                if (operation.Value.Channel != null)
                {
                    var messages = context.GetFromTempStorage<Dictionary<string, AsyncApiMessageReference>>(TempStorageKeys.OperationMessageReferences, operation.Value);
                    var channel = document.Channels.First(channel => channel.Key == operation.Value.Channel.Reference.Reference.Split("/")[^1]);
                    foreach (var message in messages)
                    {
                        channel.Value.Messages.TryAdd(message.Key, message.Value);
                        operation.Value.Messages.Add(new AsyncApiMessageReference($"#/channels/{channel.Key}/messages/{message.Key}"));
                    }
                }
            }
        }
    }
}
