namespace ByteBard.AsyncAPI.Bindings.AnypointMQ
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding for an Anypoint MQ destination.</summary>
    public class AnypointMQChannelBinding : ChannelBinding<AnypointMQChannelBinding>
    {
        public string Destination { get; set; }

        public string DestinationType { get; set; }

        public override string BindingKey => "anypointmq";

        protected override FixedFieldMap<AnypointMQChannelBinding> FixedFieldMap => new()
        {
            { "destination", (a, n) => { a.Destination = n.GetScalarValue(); } },
            { "destinationType", (a, n) => { a.DestinationType = n.GetScalarValue(); } },
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);

        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

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
