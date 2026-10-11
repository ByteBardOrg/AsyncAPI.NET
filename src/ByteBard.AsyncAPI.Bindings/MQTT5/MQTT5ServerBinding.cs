namespace ByteBard.AsyncAPI.Bindings.MQTT5
{
    using System;
    using ByteBard.AsyncAPI.Bindings.MQTT;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding class for MQTT 5 server settings. This binding is deprecated.</summary>
    [Obsolete("The MQTT 5 server binding is deprecated.")]
    public class MQTT5ServerBinding : ServerBinding<MQTT5ServerBinding>
    {
        /// <summary>Session expiration interval in seconds, or a schema describing the value.</summary>
        public MQTTIntegerOrSchema SessionExpiryInterval { get; set; }

        public override string BindingKey => "mqtt5";

        protected override FixedFieldMap<MQTT5ServerBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "sessionExpiryInterval", (a, n) => { a.SessionExpiryInterval = MQTTIntegerOrSchema.Parse(n); } },
        };

        /// <summary>
        /// Writes the MQTT 5 server binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeProperties(writer, AsyncApiVersion.AsyncApi2_0);
        /// <summary>
        /// Writes the MQTT 5 server binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer, AsyncApiVersion.AsyncApi3_0);

        /// <summary>
        /// Writes the MQTT 5 server binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <remarks>An unset binding version is omitted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer) => this.SerializeProperties(writer, AsyncApiVersion.AsyncApi2_0);

        /// <summary>
        /// Writes the MQTT 5 server binding object and extensions, using the selected version for nested schemas.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        private void SerializeProperties(IAsyncApiWriter writer, AsyncApiVersion version)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            if (this.SessionExpiryInterval != null)
            {
                writer.WritePropertyName("sessionExpiryInterval");
                this.SessionExpiryInterval.Serialize(writer, version);
            }
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
