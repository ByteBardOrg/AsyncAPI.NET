namespace ByteBard.AsyncAPI.Bindings.Kafka
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for Kafka operations.
    /// </summary>
    public class KafkaOperationBinding : OperationBinding<KafkaOperationBinding>
    {
        /// <summary>
        /// Id of the consumer group.
        /// </summary>
        public AsyncApiJsonSchema GroupId { get; set; }

        /// <summary>
        /// Id of the consumer inside a consumer group.
        /// </summary>
        public AsyncApiJsonSchema ClientId { get; set; }

        public override string BindingKey => "kafka";

        protected override FixedFieldMap<KafkaOperationBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "groupId", (a, n) => { a.GroupId = AsyncApiSchemaDeserializer.LoadSchema(n); } },
            { "clientId", (a, n) => { a.ClientId = AsyncApiSchemaDeserializer.LoadSchema(n); } },
        };

        /// <summary>
        /// Serialize to AsyncAPI V2 document without using reference.
        /// </summary>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalObject(AsyncApiConstants.GroupId, this.GroupId, (w, h) => h.SerializeV2(w));
            writer.WriteOptionalObject(AsyncApiConstants.ClientId, this.ClientId, (w, h) => h.SerializeV2(w));
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);

            writer.WriteEndObject();
        }
    }
}
