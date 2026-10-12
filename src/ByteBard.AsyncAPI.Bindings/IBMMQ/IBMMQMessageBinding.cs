namespace ByteBard.AsyncAPI.Bindings.IBMMQ
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    public class IBMMQMessageBinding : MessageBinding<IBMMQMessageBinding>
    {
        public string Type { get; set; }
        public string Headers { get; set; }
        public string Description { get; set; }
        public long? Expiry { get; set; }

        public override string BindingKey => "ibmmq";

        protected override FixedFieldMap<IBMMQMessageBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "type", (a, n) => { a.Type = n.GetScalarValue(); } },
            { "headers", (a, n) => { a.Headers = n.GetScalarValue(); } },
            { "description", (a, n) => { a.Description = n.GetScalarValue(); } },
            { "expiry", (a, n) => { a.Expiry = n.GetLongValue(); } },
        };

        /// <summary>
        /// Writes the IBM MQ message binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);
        /// <summary>
        /// Writes the IBM MQ message binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        /// <summary>
        /// Writes the IBM MQ message binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>An unset binding version is omitted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("type", this.Type);
            writer.WriteOptionalProperty("headers", this.Headers);
            writer.WriteOptionalProperty("description", this.Description);
            writer.WriteOptionalProperty("expiry", this.Expiry);
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
