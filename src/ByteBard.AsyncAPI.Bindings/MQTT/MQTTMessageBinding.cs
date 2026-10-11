namespace ByteBard.AsyncAPI.Bindings.MQTT
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for MQTT messages.
    /// </summary>
    public class MQTTMessageBinding : MessageBinding<MQTTMessageBinding>
    {
        /// <summary>
        /// Indicates the format of the payload.
        /// Either: 0 (zero) for unspecified bytes, or 1 for UTF-8 encoded character data.
        /// </summary>
        public int? PayloadFormatIndicator { get; set; }

        /// <summary>
        /// Correlation Data is used to identify the request the response message is for.
        /// </summary>
        public AsyncApiJsonSchema CorrelationData { get; set; }

        /// <summary>
        /// String describing the content type of the message payload.
        /// This should not conflict with the contentType field of the associated AsyncAPI Message object.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The topic (channel URI) for a response message.
        /// </summary>
        public string ResponseTopic
        {
            get => this.ResponseTopicValue?.Value;
            set => this.ResponseTopicValue = value == null ? null : MQTTStringOrSchema.FromString(value);
        }

        /// <summary>The string-or-schema response topic value.</summary>
        public MQTTStringOrSchema ResponseTopicValue { get; set; }

        /// <summary>
        /// Writes the MQTT message binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi2_0);
        }

        /// <summary>
        /// Writes the MQTT message binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);
        }

        /// <summary>
        /// Writes the MQTT message binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>ResponseTopicValue is written as a string or schema; nested schemas use AsyncAPI 3.0 serialization.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);
        }

        /// <summary>
        /// Writes the MQTT message binding object and extensions, using the selected version for nested schemas.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        private void SerializeProperties(IAsyncApiWriter writer, AsyncApiVersion version)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalProperty("payloadFormatIndicator", this.PayloadFormatIndicator);
            writer.WriteOptionalObject("correlationData", this.CorrelationData, (w, h) => { if (version == AsyncApiVersion.AsyncApi2_0) h.SerializeV2(w); else h.SerializeV3(w); });
            writer.WriteOptionalProperty("contentType", this.ContentType);
            if (this.ResponseTopicValue?.Value != null) writer.WriteOptionalProperty("responseTopic", this.ResponseTopicValue.Value);
            else if (this.ResponseTopicValue?.Schema != null) writer.WriteOptionalObject("responseTopic", this.ResponseTopicValue.Schema, (w, schema) => { if (version == AsyncApiVersion.AsyncApi2_0) schema.SerializeV2(w); else schema.SerializeV3(w); });
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public override string BindingKey => "mqtt";

        protected override FixedFieldMap<MQTTMessageBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "payloadFormatIndicator", (a, n) => { a.PayloadFormatIndicator = n.GetIntegerValueOrDefault(); } },
            { "correlationData", (a, n) => { a.CorrelationData = AsyncApiJsonSchemaDeserializer.LoadSchema(n); } },
            { "contentType", (a, n) => { a.ContentType = n.GetScalarValue(); } },
            { "responseTopic", (a, n) => { a.ResponseTopicValue = MQTTStringOrSchema.Parse(n); } },
        };
    }
}
