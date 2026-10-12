namespace ByteBard.AsyncAPI.Bindings.MQTT
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for MQTT operations.
    /// </summary>
    public class MQTTOperationBinding : OperationBinding<MQTTOperationBinding>
    {
        private int? qos;
        private bool? retain;

        /// <summary>
        /// Defines the Quality of Service (QoS) levels for the message flow between client and server.
        /// Its value MUST be either 0 (At most once delivery), 1 (At least once delivery), or 2 (Exactly once delivery).
        /// </summary>
        public int QoS
        {
            get => this.qos ?? 0;
            set => this.qos = value;
        }

        /// <summary>
        /// Whether the broker should retain the message or not.
        /// </summary>
        public bool Retain
        {
            get => this.retain ?? false;
            set => this.retain = value;
        }

        /// <summary>
        /// Interval in seconds or a Schema Object containing the definition of the lifetime of the message.
        /// </summary>
        public int? MessageExpiryInterval
        {
            get => this.MessageExpiryIntervalValue?.Integer;
            set => this.MessageExpiryIntervalValue = value.HasValue ? MQTTIntegerOrSchema.FromInteger(value.Value) : null;
        }

        /// <summary>The integer-or-schema message expiry interval value.</summary>
        public MQTTIntegerOrSchema MessageExpiryIntervalValue { get; set; }

        public override string BindingKey => "mqtt";

        protected override FixedFieldMap<MQTTOperationBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "qos", (a, n) => { a.QoS = n.GetIntegerValue(); } },
            { "retain", (a, n) => { a.Retain = n.GetBooleanValue(); } },
            { "messageExpiryInterval", (a, n) => { a.MessageExpiryIntervalValue = MQTTIntegerOrSchema.Parse(n); } },
        };

        /// <summary>
        /// Writes the MQTT operation binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi2_0);
        }

        /// <summary>
        /// Writes the MQTT operation binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);
        }

        /// <summary>
        /// Writes the MQTT operation binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>Unset QoS and retain values are omitted. Message expiry is written as an integer or schema.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);
        }

        /// <summary>
        /// Writes the MQTT operation binding object and extensions, using the selected version for nested schemas.
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
            writer.WriteOptionalProperty("qos", this.qos);
            writer.WriteOptionalProperty("retain", this.retain);
            if (this.MessageExpiryIntervalValue?.LongInteger is long integer) writer.WriteRequiredProperty("messageExpiryInterval", integer);
            else if (this.MessageExpiryIntervalValue?.Schema != null) writer.WriteOptionalObject("messageExpiryInterval", this.MessageExpiryIntervalValue.Schema, (w, schema) =>
            {
                if (version == AsyncApiVersion.AsyncApi2_0) schema.SerializeV2(w);
                else schema.SerializeV3(w);
            });
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
