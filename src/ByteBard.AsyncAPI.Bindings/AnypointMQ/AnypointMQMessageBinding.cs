namespace ByteBard.AsyncAPI.Bindings.AnypointMQ
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding for an Anypoint MQ message.</summary>
    public class AnypointMQMessageBinding : MessageBinding<AnypointMQMessageBinding>
    {
        /// <summary>Message headers described by a schema or schema reference.</summary>
        public AsyncApiJsonSchema Headers { get; set; }

        public override string BindingKey => "anypointmq";

        protected override FixedFieldMap<AnypointMQMessageBinding> FixedFieldMap => new()
        {
            { "headers", (a, n) => { a.Headers = AsyncApiJsonSchemaDeserializer.LoadSchema(n); } },
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalObject("headers", this.Headers, (w, h) => h.SerializeV2(w));
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }

        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalObject("headers", this.Headers, (w, h) => h.SerializeV3(w));
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
