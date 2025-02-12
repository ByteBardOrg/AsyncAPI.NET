// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.Writers;
    public class AsyncApiServer : IAsyncApiSerializable, IAsyncApiExtensible
    {
        /// <summary>
        /// REQUIRED. The server host name. It MAY include the port. This field supports Server Variables. Variable substitutions will be made when a variable is named in {braces}.
        /// </summary>
        public virtual string Host { get; set; }

        /// <summary>
        /// The path to a resource in the host. This field supports Server Variables. Variable substitutions will be made when a variable is named in {braces}.
        /// </summary>
        public virtual string PathName { get; set; }

        /// <summary>
        /// REQUIRED. The protocol this URL supports for connection.
        /// </summary>
        public virtual string Protocol { get; set; }

        /// <summary>
        /// The version of the protocol used for connection. For instance: AMQP 0.9.1, HTTP 2.0, Kafka 1.0.0, etc.
        /// </summary>
        public virtual string ProtocolVersion { get; set; }

        /// <summary>
        /// an optional string describing the host designated by the URL.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// A human-friendly title for the server.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// A short summary of the server.
        /// </summary>
        public virtual string Summary { get; set; }

        /// <summary>
        /// a map between a variable name and its value. The value is used for substitution in the server's URL template.
        /// </summary>
        public virtual IDictionary<string, AsyncApiServerVariable> Variables { get; set; } = new Dictionary<string, AsyncApiServerVariable>();

        /// <summary>
        /// a declaration of which security mechanisms can be used with this server. The list of values includes alternative security requirement objects that can be used.
        /// </summary>
        /// <remarks>
        /// The name used for each property MUST correspond to a security scheme declared in the Security Schemes under the Components Object.
        /// </remarks>
        /// TODO: how to go from scheme to requirement for V2..
        public virtual IList<AsyncApiSecurityScheme> Security { get; set; } = new List<AsyncApiSecurityScheme>();

        /// <summary>
        /// A list of tags for logical grouping and categorization of servers.
        /// </summary>
        public virtual IList<AsyncApiTag> Tags { get; set; } = new List<AsyncApiTag>();

        /// <summary>
        /// Additional external documentation for this server.
        /// </summary>
        public virtual AsyncApiExternalDocumentation ExternalDocs { get; set; }

        /// <summary>
        /// a map where the keys describe the name of the protocol and the values describe protocol-specific definitions for the server.
        /// </summary>
        public virtual AsyncApiBindings<IServerBinding> Bindings { get; set; } = new AsyncApiBindings<IServerBinding>();

        /// <inheritdoc/>
        public virtual IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public virtual void SerializeV2(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteRequiredProperty(AsyncApiConstants.Url, this.GenerateServerUrl());

            writer.WriteRequiredProperty(AsyncApiConstants.Protocol, this.Protocol);

            writer.WriteOptionalProperty(AsyncApiConstants.ProtocolVersion, this.ProtocolVersion);

            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            writer.WriteOptionalMap(AsyncApiConstants.Variables, this.Variables, (w, v) => v.SerializeV2(w));

            writer.WriteOptionalCollection(AsyncApiConstants.Security, this.Security, (w, s) => this.SerializeSecurityRequirements(w, s));

            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, s) => s.SerializeV2(w));

            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV2(w));
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        public virtual void SerializeV3(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteRequiredProperty(AsyncApiConstants.Host, this.Host);

            writer.WriteRequiredProperty(AsyncApiConstants.Protocol, this.Protocol);

            writer.WriteOptionalProperty(AsyncApiConstants.ProtocolVersion, this.ProtocolVersion);

            writer.WriteOptionalProperty(AsyncApiConstants.PathName, this.PathName);

            writer.WriteOptionalProperty(AsyncApiConstants.Description, this.Description);

            writer.WriteOptionalMap(AsyncApiConstants.Variables, this.Variables, (w, v) => v.SerializeV3(w));

            writer.WriteOptionalCollection(AsyncApiConstants.Security, this.Security, (w, s) => s.SerializeV3(w));

            writer.WriteOptionalCollection(AsyncApiConstants.Tags, this.Tags, (w, s) => s.SerializeV3(w));

            writer.WriteOptionalObject(AsyncApiConstants.Bindings, this.Bindings, (w, t) => t.SerializeV3(w));
            writer.WriteExtensions(this.Extensions);

            writer.WriteEndObject();
        }

        private string GenerateServerUrl()
        {
            var baseUri = new Uri($"{this.Protocol}{Uri.SchemeDelimiter}{this.Host}");
            return new Uri(baseUri, this.PathName).ToString();
        }

        private void SerializeSecurityRequirements(IAsyncApiWriter writer, AsyncApiSecurityScheme scheme)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (scheme is not AsyncApiSecuritySchemeReference schemeReference)
            {
                throw new AsyncApiWriterException("Cannot serialize securityScheme as V2 as it is not a Reference.");
            }

            writer.WriteStartObject();

            writer.WritePropertyName(schemeReference.Reference.FragmentId);
            writer.WriteStartArray();

            foreach (var scope in schemeReference.Scopes)
            {
                writer.WriteValue(scope);
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}