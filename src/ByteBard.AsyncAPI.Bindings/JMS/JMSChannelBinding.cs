namespace ByteBard.AsyncAPI.Bindings.JMS
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding for a JMS destination channel.</summary>
    public class JMSChannelBinding : ChannelBinding<JMSChannelBinding>
    {
        public string Destination { get; set; }

        public string DestinationType { get; set; }

        public override string BindingKey => "jms";

        protected override FixedFieldMap<JMSChannelBinding> FixedFieldMap => new()
        {
            { "destination", (a, n) => { a.Destination = n.GetScalarValue(); } },
            { "destinationType", (a, n) => { a.DestinationType = n.GetScalarValue(); } },
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
        };

        /// <summary>
        /// Writes the JMS channel binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);

        /// <summary>
        /// Writes the JMS channel binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        /// <summary>
        /// Writes the JMS channel binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>An unset binding version is omitted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("destination", this.Destination);
            writer.WriteOptionalProperty("destinationType", this.DestinationType);
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
