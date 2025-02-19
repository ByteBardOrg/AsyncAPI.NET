// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LEGO.AsyncAPI.Extensions;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// Describes the operations available on a single channel.
    /// </summary>
    public class AsyncApiChannel : IAsyncApiSerializable, IAsyncApiExtensible
    {
        /// <summary>
        /// An optional string representation of this channel's address. The address is typically the "topic name", "routing key", "event type", or "path". When null or absent, it MUST be interpreted as unknown. This is useful when the address is generated dynamically at runtime or can't be known upfront. It MAY contain Channel Address Expressions. Query parameters and fragments SHALL NOT be used, instead use bindings to define them.
        /// </summary>
        public virtual string? Address { get; set; }

        /// <summary>
        /// A map of the messages that will be sent to this channel by any application at any time. Every message sent to this channel MUST be valid against one, and only one, of the message objects defined in this map.
        /// </summary>
        public virtual IDictionary<string, AsyncApiMessage> Messages { get; set; } = new Dictionary<string, AsyncApiMessage>();

        /// <summary>
        /// A human-friendly title for the channel.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// A short summary of the channel.
        /// </summary>
        public virtual string Summary { get; set; }

        /// <summary>
        /// an optional description of this channel item. CommonMark syntax can be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// the servers on which this channel is available, specified as an optional unordered list of names (string keys) of Server Objects defined in the Servers Object (a map).
        /// </summary>
        /// <remarks>
        /// If servers is absent or empty then this channel must be available on all servers defined in the Servers Object.
        /// </remarks>
        public virtual IList<AsyncApiServerReference> Servers { get; set; } = new List<AsyncApiServerReference>();

        /// <summary>
        /// A map of the parameters included in the channel address. It MUST be present only when the address contains Channel Address Expressions.
        /// </summary>
        public virtual IDictionary<string, AsyncApiParameter> Parameters { get; set; } = new Dictionary<string, AsyncApiParameter>();

        /// <summary>
        /// A list of tags for logical grouping of channels.
        /// </summary>
        public virtual IList<AsyncApiTag> Tags { get; set; } = new List<AsyncApiTag>();

        /// <summary>
        /// Additional external documentation for this channel.
        /// </summary>
        public virtual AsyncApiExternalDocumentation ExternalDocs { get; set; }

        /// <summary>
        /// a map where the keys describe the name of the protocol and the values describe protocol-specific definitions for the channel.
        /// </summary>
        public virtual AsyncApiBindings<IChannelBinding> Bindings { get; set; } = new AsyncApiBindings<IChannelBinding>();

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            // description
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            // servers
            writer.WriteOptionalCollection(AsyncApiConstants.Servers, this.Servers.Select(s => s.Reference.FragmentId).ToList(), (w, s) => w.WriteValue(s));

            var operations = writer.Workspace.RootDocument?.Operations.Values.Where(operation => this.CheckOperationChannel(operation, writer)).ToList();

            // subscribe (Now Send)
            writer.WriteOptionalObject(AsyncApiConstants.Subscribe, operations?.FirstOrDefault(o => o.Action == AsyncApiAction.Send), (w, s) => s?.SerializeV2(w));

            // publish (Now Receive)
            writer.WriteOptionalObject(AsyncApiConstants.Publish, operations?.FirstOrDefault(o => o.Action == AsyncApiAction.Receive), (w, s) => s?.SerializeV2(w));

            // parameters
            writer.WriteOptionalMap(AsyncApiConstants.Parameters, this.Parameters, (writer, key, component) =>
            {
                if (component is AsyncApiParameterReference reference)
                {
                    reference.SerializeV2(writer);
                }
                else
                {
                    component.SerializeV2(writer);
                }
            });

            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV2(w));

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            writer.WriteOptionalProperty(AsyncApiConstants.Address, this.Address);
            writer.WriteRequiredMap(AsyncApiConstants.Messages, this.Messages, (w, k, m) => m.SerializeV3(w));
            writer.WriteOptionalProperty(AsyncApiConstants.Title, this.Title);
            writer.WriteOptionalProperty(AsyncApiConstants.Summary, this.Summary);
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteOptionalCollection(AsyncApiConstants.Servers, this.Servers, (w, s) => s.Reference.SerializeV3(w));
            if (this.Address.IsChannelAddressExpression())
            {
                writer.WriteOptionalMap(AsyncApiConstants.Parameters, this.Parameters, (w, key, p) => p.SerializeV3(w));
            }

            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.ExternalDocs, this.ExternalDocs, (w, s) => s.SerializeV2(w));
            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV2(w));
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        private bool CheckOperationChannel(AsyncApiOperation operation, IAsyncApiWriter writer)
        {
            operation.Channel.Reference.Workspace = writer.Workspace;

            return operation.Channel.Equals(this);
        }
    }
}