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
        private readonly Dictionary<string, Stream> artifactsRegistry = new(StringComparer.Ordinal);
        private readonly Dictionary<string, IAsyncApiSerializable> resolvedReferenceRegistry = new(StringComparer.Ordinal);

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
                location = componentsBaseUri + ReferenceType.Schema.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Parameters
            foreach (var item in document.Components.Parameters)
            {
                location = componentsBaseUri + ReferenceType.Parameter.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Channels
            foreach (var item in document.Components.Channels)
            {
                location = componentsBaseUri + ReferenceType.Channel.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Operations
            foreach (var item in document.Components.Operations)
            {
                location = componentsBaseUri + ReferenceType.Operation.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Servers
            foreach (var item in document.Components.Servers)
            {
                location = componentsBaseUri + ReferenceType.Server.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register ServerVariables
            foreach (var item in document.Components.ServerVariables)
            {
                location = componentsBaseUri + ReferenceType.ServerVariable.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Messages
            foreach (var item in document.Components.Messages)
            {
                location = componentsBaseUri + ReferenceType.Message.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register SecuritySchemes
            foreach (var item in document.Components.SecuritySchemes)
            {
                location = componentsBaseUri + ReferenceType.SecurityScheme.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
                this.RegisterComponent(item.Key, item.Value);
            }

            // Register Server Variables
            foreach (var item in document.Components.ServerVariables)
            {
                location = componentsBaseUri + ReferenceType.ServerVariable.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register CorrelationIds
            foreach (var item in document.Components.CorrelationIds)
            {
                location = componentsBaseUri + ReferenceType.CorrelationId.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Replies
            foreach (var item in document.Components.Replies)
            {
                location = componentsBaseUri + ReferenceType.OperationReply.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register ReplyAddresses
            foreach (var item in document.Components.ReplyAddresses)
            {
                location = componentsBaseUri + ReferenceType.OperationReplyAddress.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register ExternalDocs
            foreach (var item in document.Components.ExternalDocs)
            {
                location = componentsBaseUri + ReferenceType.ExternalDocs.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register Tags
            foreach (var item in document.Components.Tags)
            {
                location = componentsBaseUri + ReferenceType.Tag.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register OperationTraits
            foreach (var item in document.Components.OperationTraits)
            {
                location = componentsBaseUri + ReferenceType.OperationTrait.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register MessageTraits
            foreach (var item in document.Components.MessageTraits)
            {
                location = componentsBaseUri + ReferenceType.MessageTrait.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register ServerBindings
            foreach (var item in document.Components.ServerBindings)
            {
                location = componentsBaseUri + ReferenceType.ServerBindings.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register ChannelBindings
            foreach (var item in document.Components.ChannelBindings)
            {
                location = componentsBaseUri + ReferenceType.ChannelBindings.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register OperationBindings
            foreach (var item in document.Components.OperationBindings)
            {
                location = componentsBaseUri + ReferenceType.OperationBindings.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register MessageBindings
            foreach (var item in document.Components.MessageBindings)
            {
                location = componentsBaseUri + ReferenceType.MessageBindings.GetDisplayName() + "/" + JsonPointer.EscapeUriFragmentToken(item.Key);
                this.RegisterComponent(location, item.Value);
            }

            // Register references last, so one pointing at another root channel or server finds it registered.
            string channelBaseUri = "#/channels/";
            foreach (var channel in document.Channels.OrderBy(channel => channel.Value is IAsyncApiReferenceable))
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

                // A reference to a channel that is not registered (yet) is resolved by the reader later.
                if (registerableChannelValue == null)
                {
                    continue;
                }

                location = channelBaseUri + JsonPointer.EscapeUriFragmentToken(channel.Key);
                this.RegisterComponent(location, registerableChannelValue);

                foreach (var message in registerableChannelValue.Messages)
                {
                    this.RegisterComponent(location + "/messages/" + JsonPointer.EscapeUriFragmentToken(message.Key), message.Value);
                }
            }

            string serverBaseUri = "#/servers/";
            foreach (var server in document.Servers.OrderBy(server => server.Value is IAsyncApiReferenceable))
            {
                var registerableServerValue = server.Value;
                if (server.Value is IAsyncApiReferenceable serverReference)
                {
                    if (serverReference.Reference.IsExternal)
                    {
                        continue;
                    }

                    registerableServerValue = this.ResolveReference<AsyncApiServer>(serverReference.Reference);
                }

                location = serverBaseUri + JsonPointer.EscapeUriFragmentToken(server.Key);
                this.RegisterComponent(location, registerableServerValue);
            }
        }

        public bool RegisterComponent<T>(string location, T component)
        {
            var key = NormalizeLocation(location);
            if (component is IAsyncApiSerializable referenceable)
            {
                if (!this.resolvedReferenceRegistry.ContainsKey(key))
                {
                    this.resolvedReferenceRegistry[key] = referenceable;
                    return true;
                }
            }

            if (component is Stream stream)
            {
                if (!this.artifactsRegistry.ContainsKey(key))
                {
                    this.artifactsRegistry[key] = stream;
                }

                return true;
            }

            return false;
        }

        public bool Contains(string location)
        {
            var key = NormalizeLocation(location);
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
            var key = NormalizeLocation(location);
            if (this.resolvedReferenceRegistry.TryGetValue(key, out var referenceableValue))
            {
                return referenceableValue as T;
            }

            if (this.artifactsRegistry.TryGetValue(key, out var stream))
            {
                stream.Position = 0;
                return (T)(object)stream;
            }

            return default;
        }

        /// <summary>
        /// Rewrites the JSON pointer fragment of a location into one percent-encoded form (RFC 6901 section 6), so encoded
        /// and unencoded references to the same key share a registry entry, e.g. <c>#/servers/a b</c> and <c>#/servers/a%20b</c>.
        /// Locations are compared as strings, as <see cref="Uri"/> equality ignores the fragment of absolute URIs.
        /// </summary>
        private static string NormalizeLocation(string location)
        {
            var fragmentStart = location.IndexOf('#');
            if (fragmentStart < 0 || !JsonPointer.TryFromUriFragment(location.Substring(fragmentStart + 1), out var pointer))
            {
                return location;
            }

            return location.Substring(0, fragmentStart + 1) + pointer.ToUriFragment();
        }

        public void SetRootDocument(AsyncApiDocument doc)
        {
            this.RootDocument = doc;
            doc.SourceWorkspace = this;
        }

        /// <summary>
        /// Prepares this workspace for writing the document: registers its components, then adds what the workspace
        /// the document was read into resolved outside them, so references to those can still be written inline.
        /// </summary>
        internal void RegisterForWriting(AsyncApiDocument doc)
        {
            this.RootDocument = doc;
            this.RegisterComponents(doc);

            if (doc.SourceWorkspace == null || doc.SourceWorkspace == this)
            {
                return;
            }

            foreach (var resolved in doc.SourceWorkspace.resolvedReferenceRegistry)
            {
                if (!this.resolvedReferenceRegistry.ContainsKey(resolved.Key))
                {
                    this.resolvedReferenceRegistry[resolved.Key] = resolved.Value;
                }
            }
        }
    }
}
