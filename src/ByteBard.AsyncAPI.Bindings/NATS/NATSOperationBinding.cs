namespace ByteBard.AsyncAPI.Bindings.NATS
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding class for NATS operations.</summary>
    public class NATSOperationBinding : OperationBinding<NATSOperationBinding>
    {
        /// <summary>Queue group name.</summary>
        public string Queue { get; set; }

        public override string BindingKey => "nats";

        protected override FixedFieldMap<NATSOperationBinding> FixedFieldMap => new()
        {
            { "queue", (a, n) => a.Queue = n.GetScalarValue() },
            { "bindingVersion", (a, n) => a.BindingVersion = n.GetScalarValue() },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeProperties(writer);
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("queue", this.Queue);
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
