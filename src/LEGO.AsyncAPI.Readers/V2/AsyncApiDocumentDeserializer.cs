// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers
{
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Readers.ParseNodes;
    using System.Collections.Generic;

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
            return document;
        }
    }
}
