namespace ByteBard.AsyncAPI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;

    public class AsyncApiWorkspace
    {
        private readonly Dictionary<Uri, Stream> artifactsRegistry = new();
        private readonly Dictionary<Uri, IAsyncApiSerializable> resolvedReferenceRegistry = new();

        public AsyncApiDocument RootDocument { get; private set; }

        /// <summary>
        /// Stack for tracking parent context during serialization.
        /// Allows bindings to access their parent operation, channel, or message.
        /// </summary>
        public Stack<object> SerializationContext { get; } = new();

        /// <summary>
        /// Gets the first item of the specified type from the serialization context.
        /// </summary>
        /// <typeparam name="T">The type to find in the context.</typeparam>
        /// <returns>The first matching item, or default if not found.</returns>
        public T GetSerializationContext<T>()
            where T : class
        {
            return this.SerializationContext.OfType<T>().FirstOrDefault();
        }

        public void RegisterComponents(AsyncApiDocument document)
        {
            if (document?.Components == null)
            {
                return;
            }

            string componentsBaseUri = "#/components/";
            string location;

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

            // Register Operations
            foreach (var item in document.Components.Operations)
            {
                location = componentsBaseUri + ReferenceType.Operation.GetDisplayName() + "/" + item.Key;
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

            // Register Server Variables
            foreach (var item in document.Components.ServerVariables)
            {
                location = componentsBaseUri + ReferenceType.ServerVariable.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register CorrelationIds
            foreach (var item in document.Components.CorrelationIds)
            {
                location = componentsBaseUri + ReferenceType.CorrelationId.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register Replies
            foreach (var item in document.Components.Replies)
            {
                location = componentsBaseUri + ReferenceType.OperationReply.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register ReplyAddresses
            foreach (var item in document.Components.ReplyAddresses)
            {
                location = componentsBaseUri + ReferenceType.OperationReplyAddress.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register ExternalDocs
            foreach (var item in document.Components.ExternalDocs)
            {
                location = componentsBaseUri + ReferenceType.ExternalDocs.GetDisplayName() + "/" + item.Key;
                this.RegisterComponent(location, item.Value);
            }

            // Register Tags
            foreach (var item in document.Components.Tags)
            {
                location = componentsBaseUri + ReferenceType.Tag.GetDisplayName() + "/" + item.Key;
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

            string channelBaseUri = "#/channels/";
            foreach (var channel in document.Channels)
            {
                var registerableChannelValue = channel.Value;
                if (channel.Value is IAsyncApiReferenceable reference)
                {
                    if (reference.Reference.IsExternal)
                    {
                        continue;
                    }

                    registerableChannelValue = this.ResolveReference<AsyncApiChannel>(reference.Reference);
                }

                location = channelBaseUri + channel.Key;
                this.RegisterComponent(location, registerableChannelValue);

                foreach (var message in registerableChannelValue.Messages)
                {
                    this.RegisterComponent(location + "/messages/" + message.Key, message.Value);
                }
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
