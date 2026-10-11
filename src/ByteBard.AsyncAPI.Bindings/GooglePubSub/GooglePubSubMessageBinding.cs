namespace ByteBard.AsyncAPI.Bindings.GooglePubSub
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    public class GooglePubSubMessageBinding : MessageBinding<GooglePubSubMessageBinding>
    {
        public Dictionary<string, string> Attributes { get; set; }
        public string OrderingKey { get; set; }
        public MessageSchema Schema { get; set; }

        public override string BindingKey => "googlepubsub";

        protected override FixedFieldMap<GooglePubSubMessageBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "attributes", (a, n) => { a.Attributes = n.CreateSimpleMap(s => s.GetScalarValue()); } },
            { "orderingKey", (a, n) => { a.OrderingKey = n.GetScalarValue(); } },
            { "schema", (a, n) => { a.Schema = n.ParseMap(schemaFields); } },
        };

        private static readonly FixedFieldMap<MessageSchema> schemaFields = new()
        {
            { "name", (a, n) => { a.Name = n.GetScalarValue(); } },
        };

        /// <summary>
        /// Writes the Google Pub/Sub message binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);
        /// <summary>
        /// Writes the Google Pub/Sub message binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        /// <summary>
        /// Writes the Google Pub/Sub message binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>An unset binding version is omitted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalMap("attributes", this.Attributes, (w, v) => w.WriteValue(v));
            writer.WriteOptionalProperty("orderingKey", this.OrderingKey);
            writer.WriteOptionalObject("schema", this.Schema, (w, v) => v.Serialize(w));
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
