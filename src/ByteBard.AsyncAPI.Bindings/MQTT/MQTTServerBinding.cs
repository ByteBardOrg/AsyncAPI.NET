namespace ByteBard.AsyncAPI.Bindings.MQTT
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for MQTT channel settings.
    /// </summary>
    public class MQTTServerBinding : ServerBinding<MQTTServerBinding>
    {
        /// <summary>
        /// The client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Whether to create a persistent connection or not.
        /// When false, the connection will be persistent.
        /// This is called clean start in MQTTv5.
        /// </summary>
        public bool? CleanSession { get; set; }

        /// <summary>
        /// Last Will and Testament configuration.
        /// </summary>
        public LastWill LastWill { get; set; }

        /// <summary>
        /// Interval in seconds of the longest period of time
        /// the broker and the client can endure without sending a message.
        /// </summary>
        public int? KeepAlive { get; set; }

        /// <summary>
        /// Interval in seconds the broker maintains a session
        /// for a disconnected client until this interval expires.
        /// Either an <see cref="MQTTInteger"/> or an <see cref="MQTTIntegerSchema"/>.
        /// </summary>
        public MQTTIntegerOrSchema SessionExpiryInterval { get; set; }

        /// <summary>
        /// Number of bytes representing the maximum packet size
        /// the client is willing to accept.
        /// Either an <see cref="MQTTInteger"/> or an <see cref="MQTTIntegerSchema"/>.
        /// </summary>
        public MQTTIntegerOrSchema MaximumPacketSize { get; set; }

        public override string BindingKey => "mqtt";

        protected override FixedFieldMap<MQTTServerBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "clientId", (a, n) => { a.ClientId = n.GetScalarValue(); } },
            { "cleanSession", (a, n) => { a.CleanSession = n.GetBooleanValue(); } },
            { "lastWill", (a, n) => { a.LastWill = n.ParseMap(LastWillFixedFields); } },
            { "keepAlive", (a, n) => { a.KeepAlive = n.GetIntegerValue(); } },
            { "sessionExpiryInterval", (a, n) => { a.SessionExpiryInterval = MQTTIntegerOrSchema.Parse(n); } },
            { "maximumPacketSize", (a, n) => { a.MaximumPacketSize = MQTTIntegerOrSchema.Parse(n); } },
        };

        private static FixedFieldMap<LastWill> LastWillFixedFields = new()
        {
            { "topic", (a, n) => { a.Topic = n.GetScalarValue(); } },
            { "qos", (a, n) => { a.QoS = (uint?)n.GetIntegerValue(); } },
            { "message", (a, n) => { a.Message = n.GetScalarValue(); } },
            { "retain", (a, n) => { a.Retain = n.GetBooleanValue(); } },
        };

        /// <summary>
        /// Writes the MQTT server binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi2_0);
        }

        /// <summary>
        /// Writes the MQTT server binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);
        }

        /// <summary>
        /// Writes the MQTT server binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>An unset client ID is omitted. Session expiry and maximum packet size are written as integers or schemas.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);
        }

        /// <summary>
        /// Writes the MQTT server binding object and extensions, using the selected version for nested schemas.
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
            writer.WriteOptionalProperty("clientId", this.ClientId);
            writer.WriteOptionalProperty("cleanSession", this.CleanSession);
            writer.WriteOptionalObject("lastWill", this.LastWill, (w, l) => l.Serialize(w));
            writer.WriteOptionalProperty("keepAlive", this.KeepAlive);
            this.WriteIntegerOrSchema(writer, "sessionExpiryInterval", this.SessionExpiryInterval, version);
            this.WriteIntegerOrSchema(writer, "maximumPacketSize", this.MaximumPacketSize, version);
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Writes a named integer or schema property, omitting it when the value is null.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="name">The property name to write.</param>
        /// <param name="value">The integer or schema to write, or null to omit the property.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        /// <exception cref="ArgumentNullException">A value is present and <paramref name="writer"/> or <paramref name="name"/> is null.</exception>
        private void WriteIntegerOrSchema(IAsyncApiWriter writer, string name, MQTTIntegerOrSchema value, AsyncApiVersion version)
        {
            if (value == null)
            {
                return;
            }

            writer.WritePropertyName(name);
            value.Serialize(writer, version);
        }
    }
}
