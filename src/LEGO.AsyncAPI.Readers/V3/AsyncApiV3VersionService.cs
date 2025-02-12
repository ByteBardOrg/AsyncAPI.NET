// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Readers.V3
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Exceptions;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Readers.Interface;
    using LEGO.AsyncAPI.Readers.ParseNodes;

    internal class AsyncApiV3VersionService : IAsyncApiVersionService
    {
        public AsyncApiDiagnostic Diagnostic { get; }

        /// <summary>
        /// Create Parsing Context.
        /// </summary>
        /// <param name="diagnostic">Provide instance for diagnostic object for collecting and accessing information about the parsing.</param>
        public AsyncApiV3VersionService(AsyncApiDiagnostic diagnostic)
        {
            this.Diagnostic = diagnostic;
        }

        private IDictionary<Type, Func<ParseNode, object>> loaders = new Dictionary<Type, Func<ParseNode, object>>
        {
            [typeof(AsyncApiAny)] = AsyncApiV3Deserializer.LoadAny,
            [typeof(AsyncApiComponents)] = AsyncApiV3Deserializer.LoadComponents,
            [typeof(AsyncApiExternalDocumentation)] = AsyncApiV3Deserializer.LoadExternalDocs,
            [typeof(AsyncApiInfo)] = AsyncApiV3Deserializer.LoadInfo,
            [typeof(AsyncApiLicense)] = AsyncApiV3Deserializer.LoadLicense,
            [typeof(AsyncApiOAuthFlow)] = AsyncApiV3Deserializer.LoadOAuthFlow,
            [typeof(AsyncApiOAuthFlows)] = AsyncApiV3Deserializer.LoadOAuthFlows,
            [typeof(AsyncApiOperation)] = AsyncApiV3Deserializer.LoadOperation,
            [typeof(AsyncApiOperationReply)] = AsyncApiV3Deserializer.LoadOperationReply,
            [typeof(AsyncApiOperationReplyAddress)] = AsyncApiV3Deserializer.LoadOperationReplyAddress,
            [typeof(AsyncApiParameter)] = AsyncApiV3Deserializer.LoadParameter,
            [typeof(AsyncApiJsonSchema)] = AsyncApiSchemaDeserializer.LoadSchema,
            [typeof(AsyncApiAvroSchema)] = AsyncApiAvroSchemaDeserializer.LoadSchema,
            [typeof(AsyncApiJsonSchema)] = AsyncApiV3Deserializer.LoadJsonSchemaPayload,
            [typeof(AsyncApiAvroSchema)] = AsyncApiV3Deserializer.LoadAvroPayload,
            [typeof(AsyncApiSecurityScheme)] = AsyncApiV3Deserializer.LoadSecurityScheme,
            [typeof(AsyncApiMultiFormatSchema)] = AsyncApiV3Deserializer.LoadMultiFormatSchema,
            [typeof(AsyncApiServer)] = AsyncApiV3Deserializer.LoadServer,
            [typeof(AsyncApiServerVariable)] = AsyncApiV3Deserializer.LoadServerVariable,
            [typeof(AsyncApiTag)] = AsyncApiV3Deserializer.LoadTag,
            [typeof(AsyncApiMessage)] = AsyncApiV3Deserializer.LoadMessage,
            [typeof(AsyncApiMessageTrait)] = AsyncApiV3Deserializer.LoadMessageTrait,
            [typeof(AsyncApiChannel)] = AsyncApiV3Deserializer.LoadChannel,
            [typeof(AsyncApiBindings<IServerBinding>)] = AsyncApiV3Deserializer.LoadServerBindings,
            [typeof(AsyncApiBindings<IChannelBinding>)] = AsyncApiV3Deserializer.LoadChannelBindings,
            [typeof(AsyncApiBindings<IMessageBinding>)] = AsyncApiV3Deserializer.LoadMessageBindings,
            [typeof(AsyncApiBindings<IOperationBinding>)] = AsyncApiV3Deserializer.LoadOperationBindings,
        };

        /// <summary>
        /// Parse the string to a <see cref="AsyncApiReference"/> object.
        /// </summary>
        /// <param name="reference">The URL of the reference.</param>
        /// <param name="type">The type of object referenced based on the context of the reference.</param>
        public AsyncApiReference ConvertToAsyncApiReference(
            string reference,
            ReferenceType? type)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new AsyncApiException($"The reference string '{reference}' has invalid format.");
            }

            try
            {
                return new AsyncApiReference(reference, type);
            }
            catch (AsyncApiException ex)
            {
                this.Diagnostic.Errors.Add(new AsyncApiError(ex));
                return null;
            }
        }

        public AsyncApiDocument LoadDocument(RootNode rootNode)
        {
            return AsyncApiV2Deserializer.LoadAsyncApi(rootNode);
        }

        public T LoadElement<T>(ParseNode node)
            where T : IAsyncApiElement
        {
            return (T)this.loaders[typeof(T)](node);
        }
    }
}