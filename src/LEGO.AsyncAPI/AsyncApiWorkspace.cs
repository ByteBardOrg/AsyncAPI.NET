// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using LEGO.AsyncAPI.Models;
    using LEGO.AsyncAPI.Models.Interfaces;

    public class AsyncApiWorkspace
    {
        private readonly Dictionary<Uri, Stream> artifactsRegistry = new();
        private readonly Dictionary<Uri, IAsyncApiSerializable> resolvedReferenceRegistry = new();

        public AsyncApiDocument RootDocument { get; private set; }

        public void RegisterComponents(AsyncApiDocument document)
        {
            if (document?.Components == null)
            {
                return;
            }

            string componentsBaseUri = "#/components/";
            string channelBaseUri = "#/channels/";
            string location;

            foreach (var channel in document.Channels)
            {
                location = channelBaseUri + channel.Key;
                this.RegisterComponent(location, channel.Value);

                foreach (var message in channel.Value.Messages)
                {
                    location = location + "/messages/" + message.Key;
                    this.RegisterComponent(location, message.Value);
                }
            }

            // Register Schema
            foreach (var item in document.Components.Schemas)
            {
                location = componentsBaseUri + ReferenceType.Schema.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register Parameters
            foreach (var item in document.Components.Parameters)
            {
                location = componentsBaseUri + ReferenceType.Parameter.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register Channels
            foreach (var item in document.Components.Channels)
            {
                location = componentsBaseUri + ReferenceType.Channel.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register Servers
            foreach (var item in document.Components.Servers)
            {
                location = componentsBaseUri + ReferenceType.Server.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register ServerVariables
            foreach (var item in document.Components.ServerVariables)
            {
                location = componentsBaseUri + ReferenceType.ServerVariable.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register Messages
            foreach (var item in document.Components.Messages)
            {
                location = componentsBaseUri + ReferenceType.Message.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register SecuritySchemes
            foreach (var item in document.Components.SecuritySchemes)
            {
                location = componentsBaseUri + ReferenceType.SecurityScheme.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
                this.RegisterComponent(item.Key, item.Value);
            }

            // Register Parameters
            foreach (var item in document.Components.Parameters)
            {
                location = componentsBaseUri + ReferenceType.Parameter.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register CorrelationIds
            foreach (var item in document.Components.CorrelationIds)
            {
                location = componentsBaseUri + ReferenceType.CorrelationId.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register OperationTraits
            foreach (var item in document.Components.OperationTraits)
            {
                location = componentsBaseUri + ReferenceType.OperationTrait.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register MessageTraits
            foreach (var item in document.Components.MessageTraits)
            {
                location = componentsBaseUri + ReferenceType.MessageTrait.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register ServerBindings
            foreach (var item in document.Components.ServerBindings)
            {
                location = componentsBaseUri + ReferenceType.ServerBindings.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register ChannelBindings
            foreach (var item in document.Components.ChannelBindings)
            {
                location = componentsBaseUri + ReferenceType.ChannelBindings.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register OperationBindings
            foreach (var item in document.Components.OperationBindings)
            {
                location = componentsBaseUri + ReferenceType.OperationBindings.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register MessageBindings
            foreach (var item in document.Components.MessageBindings)
            {
                location = componentsBaseUri + ReferenceType.MessageBindings.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }
        }

        public bool RegisterComponent<T>(string location, T component)
        {
            var uri = this.ToLocationUrl(location);
            if (component is IAsyncApiSerializable referenceable)
            {
                if (!this.resolvedReferenceRegistry.ContainsKey(uri))
                {
                    this.resolvedReferenceRegistry[uri] = referenceable;
                    return true;
                }
            }

            if (component is Stream stream)
            {
                if (!this.artifactsRegistry.ContainsKey(uri))
                {
                    this.artifactsRegistry[uri] = stream;
                }
                return true;
            }

            return false;
        }

        public bool Contains(string location)
        {
            var key = this.ToLocationUrl(location);
            return this.resolvedReferenceRegistry.ContainsKey(key) || this.artifactsRegistry.ContainsKey(key);
        }

        public T ResolveReference<T>(AsyncApiReference reference)
            where T : class
        {
            return this.ResolveReference<T>(reference.Reference);
        }

        public T ResolveReference<T>(string location)
            where T : class
        {
            var uri = this.ToLocationUrl(location);
            if (this.resolvedReferenceRegistry.TryGetValue(uri, out var referenceableValue))
            {
                return referenceableValue as T;
            }

            if (this.artifactsRegistry.TryGetValue(uri, out var stream))
            {
                stream.Position = 0;
                return (T)(object)stream;
            }

            return default;
        }

        private Uri ToLocationUrl(string location)
        {
            return new(location, UriKind.RelativeOrAbsolute);
        }

        public void SetRootDocument(AsyncApiDocument doc)
        {
            this.RootDocument = doc;
        }
    }
}
