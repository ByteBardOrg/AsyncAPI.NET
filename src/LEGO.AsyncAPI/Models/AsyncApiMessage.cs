﻿// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// Describes a message received on a given channel and operation.
    /// </summary>
    public class AsyncApiMessage : IAsyncApiExtensible, IAsyncApiSerializable
    {
        /// <summary>
        /// schema definition of the application headers. Schema MUST be of type "object".
        /// </summary>
        public virtual AsyncApiMultiFormatSchema Headers { get; set; }

        /// <summary>
        /// definition of the message payload.
        /// </summary>
        /// <remarks>
        /// If this is a Schema Object, then the schemaFormat will be assumed to be "application/vnd.aai.asyncapi+json;version=asyncapi" where the version is equal to the AsyncAPI Version String.
        /// </remarks>
        public virtual AsyncApiMultiFormatSchema Payload { get; set; }

        /// <summary>
        /// definition of the correlation ID used for message tracing or matching.
        /// </summary>
        public virtual AsyncApiCorrelationId CorrelationId { get; set; }

        /// <summary>
        /// the content type to use when encoding/decoding a message's payload.
        /// </summary>
        public virtual string ContentType { get; set; }

        /// <summary>
        /// a machine-friendly name for the message.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// a human-friendly title for the message.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// a short summary of what the message is about.
        /// </summary>
        public virtual string Summary { get; set; }

        /// <summary>
        /// a verbose explanation of the message. CommonMark syntax can be used for rich text representation.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// a list of tags for API documentation control. Tags can be used for logical grouping of messages.
        /// </summary>
        public virtual IList<AsyncApiTag> Tags { get; set; } = new List<AsyncApiTag>();

        /// <summary>
        /// additional external documentation for this message.
        /// </summary>
        public virtual AsyncApiExternalDocumentation ExternalDocs { get; set; }

        /// <summary>
        /// a map where the keys describe the name of the protocol and the values describe protocol-specific definitions for the message.
        /// </summary>
        public virtual AsyncApiBindings<IMessageBinding> Bindings { get; set; } = new AsyncApiBindings<IMessageBinding>();

        /// <summary>
        /// list of examples.
        /// </summary>
        public virtual IList<AsyncApiMessageExample> Examples { get; set; } = new List<AsyncApiMessageExample>();

        /// <summary>
        /// a list of traits to apply to the message object. Traits MUST be merged into the message object using the JSON Merge Patch algorithm in the same order they are defined here. The resulting object MUST be a valid Message Object.
        /// </summary>
        public virtual IList<AsyncApiMessageTrait> Traits { get; set; } = new List<AsyncApiMessageTrait>();

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalObject(AsyncApiConstants.Headers, this.Headers, (w, h) => h.Schema.SerializeV2(w));
            writer.WriteOptionalObject(AsyncApiConstants.Payload, this.Payload, (w, p) => p.SerializeV2(w));
            writer.WriteOptionalObject(AsyncApiConstants.CorrelationId, this.CorrelationId, (w, c) => c.SerializeV2(w));
            writer.WriteOptionalProperty(AsyncApiConstants.SchemaFormat, this.Payload.SchemaFormat);
            writer.WriteOptionalProperty(AsyncApiConstants.ContentType, this.ContentType);
            writer.WriteOptionalProperty(AsyncApiConstants.Name, this.Name);
            writer.WriteOptionalProperty(AsyncApiConstants.Title, this.Title);
            writer.WriteOptionalProperty(AsyncApiConstants.Summary, this.Summary);
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, t) => t.SerializeV2(w));
            writer.WriteOptionalObject(AsyncApiConstants.ExternalDocs, this.ExternalDocs, (w, e) => e.SerializeV2(w));

            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV2(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Examples, this.Examples, (w, e) => e.SerializeV2(w));

            writer.WriteOptionalCollection(AsyncApiConstants.Traits, this.Traits, (w, t) => t.SerializeV2(w));
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
            writer.WriteOptionalObject(AsyncApiConstants.Headers, this.Headers, (w, h) => h.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.Payload, this.Payload, (w, p) => p.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.CorrelationId, this.CorrelationId, (w, c) => c.SerializeV3(w));
            writer.WriteOptionalProperty(AsyncApiConstants.ContentType, this.ContentType);
            writer.WriteOptionalProperty(AsyncApiConstants.Name, this.Name);
            writer.WriteOptionalProperty(AsyncApiConstants.Title, this.Title);
            writer.WriteOptionalProperty(AsyncApiConstants.Summary, this.Summary);
            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);
            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalObject(AsyncApiConstants.ExternalDocs, this.ExternalDocs, (w, e) => e.SerializeV3(w));

            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV3(w));
            writer.WriteOptionalCollection(AsyncApiConstants.Examples, this.Examples, (w, e) => e.SerializeV3(w));

            writer.WriteOptionalCollection(AsyncApiConstants.Traits, this.Traits, (w, t) => t.SerializeV3(w));
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}