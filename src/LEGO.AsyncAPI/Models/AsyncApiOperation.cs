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
    /// Describes a publish or a subscribe operation. This provides a place to document how and why messages are sent and received.
    /// </summary>
    public class AsyncApiOperation : IAsyncApiSerializable, IAsyncApiExtensible
    {
        /// <summary>
        /// Required. Use send when it's expected that the application will send a message to the given channel, and receive when the application should expect receiving messages from the given channel.
        /// </summary>
        public virtual AsyncApiAction Action { get; set; }

        /// <summary>
        /// Required. A $ref pointer to the definition of the channel in which this operation is performed. If the operation is located in the root Operations Object, it MUST point to a channel definition located in the root Channels Object, and MUST NOT point to a channel definition located in the Components Object or anywhere else. If the operation is located in the Components Object, it MAY point to a Channel Object in any location. Please note the channel property value MUST be a Reference Object and, therefore, MUST NOT contain a Channel Object. However, it is RECOMMENDED that parsers (or other software) dereference this property for a better development experience.
        /// </summary>
        public virtual AsyncApiChannelReference Channel { get; set; }

        /// <summary>
        /// unique string used to identify the operation.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// a short summary of what the operation is about.
        /// </summary>
        public virtual string Summary { get; set; }

        /// <summary>
        /// a verbose explanation of the operation. CommonMark syntax can be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// A declaration of which security mechanisms can be used with this server. The list of values includes alternative security requirement objects that can be used. Only one of the security requirement objects need to be satisfied to authorize a connection or operation.
        /// </summary>
        public virtual IList<AsyncApiSecurityScheme> Security { get; set; } = new List<AsyncApiSecurityScheme>();

        /// <summary>
        /// a list of tags for API documentation control. Tags can be used for logical grouping of operations.
        /// </summary>
        public virtual IList<AsyncApiTag> Tags { get; set; } = new List<AsyncApiTag>();

        /// <summary>
        /// additional external documentation for this operation.
        /// </summary>
        public virtual AsyncApiExternalDocumentation ExternalDocs { get; set; }

        /// <summary>
        /// a map where the keys describe the name of the protocol and the values describe protocol-specific definitions for the operation.
        /// </summary>
        public virtual AsyncApiBindings<IOperationBinding> Bindings { get; set; } = new AsyncApiBindings<IOperationBinding>();

        /// <summary>
        /// a list of traits to apply to the operation object.
        /// </summary>
        public virtual IList<AsyncApiOperationTrait> Traits { get; set; } = new List<AsyncApiOperationTrait>();

        /// <summary>
        /// A list of $ref pointers pointing to the supported Message Objects that can be processed by this operation. It MUST contain a subset of the messages defined in the channel referenced in this operation, and MUST NOT point to a subset of message definitions located in the Messages Object in the Components Object or anywhere else. Every message processed by this operation MUST be valid against one, and only one, of the message objects referenced in this list. Please note the messages property value MUST be a list of Reference Objects and, therefore, MUST NOT contain Message Objects. However, it is RECOMMENDED that parsers (or other software) dereference this property for a better development experience.
        /// </summary>
        public virtual IList<AsyncApiMessageReference> Messages { get; set; } = new List<AsyncApiMessageReference>();

        /// <summary>
        /// The definition of the reply in a request-reply operation.
        /// </summary>
        public virtual AsyncApiOperationReply Reply { get; set; }

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();

            var operationId = writer.Workspace.RootDocument.Operations.FirstOrDefault(pair => pair.Value == this).Key;

            writer.WriteOptionalProperty(AsyncApiConstants.OperationId, operationId);
            writer.WriteOptionalProperty(AsyncApiConstants.Summary, this.Summary);
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteOptionalCollection(AsyncApiConstants.Security, this.Security, (w, t) => this.SerializeAsSecurityRequirement(t, w));
            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, t) => t.SerializeV2(w));
            writer.WriteOptionalObject(AsyncApiConstants.ExternalDocs, this.ExternalDocs, (w, e) => e.SerializeV2(w));

            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV2(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Traits, this.Traits, (w, t) => t.SerializeV2(w));
            if (this.Messages.Count > 1)
            {
                writer.WritePropertyName(AsyncApiConstants.Message);
                writer.WriteStartObject();
                writer.WriteOptionalCollection(AsyncApiConstants.OneOf, this.Messages, (w, t) => t.SerializeV2(w));
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteOptionalObject(AsyncApiConstants.Message, this.Messages.FirstOrDefault(), (w, m) => m.SerializeV2(w));
            }

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
            writer.WriteRequiredProperty(AsyncApiConstants.Action, this.Action.GetDisplayName());
            writer.WriteRequiredObject(AsyncApiConstants.Channel, this.Channel, (w, c) => c.Reference.SerializeV3(w));
            writer.WriteOptionalProperty(AsyncApiConstants.Title, this.Title);
            writer.WriteOptionalProperty(AsyncApiConstants.Summary, this.Summary);
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteOptionalCollection(AsyncApiConstants.Security, this.Security, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.ExternalDocs, this.ExternalDocs, (w, e) => e.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Traits, this.Traits, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Messages, this.Messages, (w, m) => m.Reference.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.Reply, this.Reply, (w, t) => t.SerializeV3(w));
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        private void SerializeAsSecurityRequirement(AsyncApiSecurityScheme scheme, IAsyncApiWriter w)
        {
            if (scheme is not AsyncApiSecuritySchemeReference schemeReference)
            {
                throw new AsyncApiWriterException("Cannot serialize securityScheme as V2 as it is not a Reference.");
            }

            schemeReference.SerializeAsSecurityRequirement(w);
        }
    }
}