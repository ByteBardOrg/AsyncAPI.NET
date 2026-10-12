namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>A destination configured for a Solace operation.</summary>
    public class SolaceDestination : IAsyncApiElement
    {
        public string BindingVersion { get; set; }
        public string DestinationType { get; set; }
        public string DeliveryMode { get; set; }
        public SolaceQueue Queue { get; set; }
        public SolaceTopic Topic { get; set; }

        /// <summary>
        /// Writes the destination object with any queue, topic, and binding version settings.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("destinationType", this.DestinationType);
            writer.WriteOptionalProperty("deliveryMode", this.DeliveryMode);
            writer.WriteOptionalObject("queue", this.Queue, (w, q) => q.Serialize(w));
            writer.WriteOptionalObject("topic", this.Topic, (w, t) => t.Serialize(w));
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteEndObject();
        }

        internal static FixedFieldMap<SolaceDestination> Fields => new()
        {
            { "bindingVersion", (a, n) => a.BindingVersion = n.GetScalarValue() },
            { "destinationType", (a, n) => a.DestinationType = n.GetScalarValue() },
            { "deliveryMode", (a, n) => a.DeliveryMode = n.GetScalarValue() },
            { "queue", (a, n) => a.Queue = n.ParseMap(SolaceQueue.Fields) },
            { "topic", (a, n) => a.Topic = n.ParseMap(SolaceTopic.Fields) },
        };
    }
}
