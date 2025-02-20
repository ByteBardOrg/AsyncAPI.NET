// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;

    /// <summary>
    /// This is the root document object for the API specification. It combines resource listing and API declaration together into one document.
    /// </summary>
    public class AsyncApiDocument : IAsyncApiExtensible, IAsyncApiSerializable
    {
        /// <summary>
        /// REQUIRED. Specifies the AsyncAPI Specification version being used.
        /// </summary>
        public string Asyncapi { get; set; }

        /// <summary>
        /// Identifier of the application the AsyncAPI document is defining.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// REQUIRED. Provides metadata about the API. The metadata can be used by the clients if needed.
        /// </summary>
        public AsyncApiInfo Info { get; set; }

        /// <summary>
        /// provides connection details of servers.
        /// </summary>
        public IDictionary<string, AsyncApiServer> Servers { get; set; } = new Dictionary<string, AsyncApiServer>();

        /// <summary>
        /// default content type to use when encoding/decoding a message's payload.
        /// </summary>
        /// <remarks>
        /// A string representing the default content type to use when encoding/decoding a message's payload.
        /// The value MUST be a specific media type (e.g. application/json). This value MUST be used by schema parsers when the contentType property is omitted.
        /// </remarks>
        public string DefaultContentType { get; set; }

        /// <summary>
        /// The channels used by this application.
        /// </summary>
        public IDictionary<string, AsyncApiChannel> Channels { get; set; } = new Dictionary<string, AsyncApiChannel>();

        /// <summary>
        /// The operations this application MUST implement.
        /// </summary> 
        public IDictionary<string, AsyncApiOperation> Operations { get; set; } = new Dictionary<string, AsyncApiOperation>();

        /// <summary>
        /// an element to hold various schemas for the specification.
        /// </summary>
        public AsyncApiComponents Components { get; set; }

        /// <inheritdoc/>
        public IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Workspace.SetRootDocument(this);
            writer.Workspace.RegisterComponents(this);

            writer.WriteStartObject();

            // asyncApi
            writer.WriteRequiredProperty(AsyncApiConstants.AsyncApi, "2.6.0");

            // info
            writer.WriteRequiredObject(AsyncApiConstants.Info, this.Info, (w, i) => i.SerializeV2(w));

            // id
            writer.WriteOptionalProperty(AsyncApiConstants.Id, this.Id);

            // servers
            writer.WriteOptionalMap(AsyncApiConstants.Servers, this.Servers, (writer, component) => component.SerializeV2(writer));

            // content type
            writer.WriteOptionalProperty(AsyncApiConstants.DefaultContentType, this.DefaultContentType);

            // channels
            writer.WriteRequiredMap(AsyncApiConstants.Channels, this.Channels, (channel) => channel.Address, (writer, key, component) => component.SerializeV2(writer));

            // components
            writer.WriteOptionalObject(AsyncApiConstants.Components, this.Components, (w, c) => c.SerializeV2(w));

            // tags
            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Info.Tags, (w, t) => t.SerializeV2(w));

            // external docs
            writer.WriteOptionalObject(AsyncApiConstants.ExternalDocs, this.Info.ExternalDocs, (w, e) => e.SerializeV2(w));

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        public void SerializeV3(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Workspace.SetRootDocument(this);
            writer.Workspace.RegisterComponents(this);

            writer.WriteStartObject();

            // asyncApi
            writer.WriteRequiredProperty(AsyncApiConstants.AsyncApi, "3.0.0");

            // info
            writer.WriteRequiredObject(AsyncApiConstants.Info, this.Info, (w, i) => i.SerializeV3(w));

            // id
            writer.WriteOptionalProperty(AsyncApiConstants.Id, this.Id);

            // servers
            writer.WriteOptionalMap(AsyncApiConstants.Servers, this.Servers, (writer, key, server) => server.SerializeV3(writer));

            // content type
            writer.WriteOptionalProperty(AsyncApiConstants.DefaultContentType, this.DefaultContentType);

            // channels
            writer.WriteOptionalMap(AsyncApiConstants.Channels, this.Channels, (writer, key, channel) => channel.SerializeV3(writer));

            // operations
            writer.WriteOptionalMap(AsyncApiConstants.Operations, this.Operations, (writer, key, operation) => operation.SerializeV3(writer));

            // components
            writer.WriteOptionalObject(AsyncApiConstants.Components, this.Components, (w, component) => component.SerializeV3(w));

            // extensions
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }
    }
}
