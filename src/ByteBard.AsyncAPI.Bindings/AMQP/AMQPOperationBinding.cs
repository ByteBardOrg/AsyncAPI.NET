namespace ByteBard.AsyncAPI.Bindings.AMQP
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// Binding class for AMQP operations.
    /// </summary>
    public class AMQPOperationBinding : OperationBinding<AMQPOperationBinding>
    {
        /// <summary>
        /// TTL (Time-To-Live) for the message. It MUST be greater than or equal to zero.
        /// </summary>
        public uint Expiration { get; set; }

        /// <summary>
        /// Identifies the user who has sent the message.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The routing keys the message should be routed to at the time of publishing.
        /// </summary>
        public List<string> Cc { get; set; } = new List<string>();

        /// <summary>
        /// A priority for the message.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Delivery mode of the message. Its value MUST be either 1 (transient) or 2 (persistent).
        /// </summary>
        public DeliveryMode DeliveryMode { get; set; }

        /// <summary>
        /// Whether the message is mandatory or not.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Like cc but consumers will not receive this information.
        /// </summary>
        public List<string> Bcc { get; set; } = new List<string>();

        /// <summary>
        /// Whether the message should include a timestamp or not.
        /// </summary>
        public bool Timestamp { get; set; }

        /// <summary>
        /// Whether the consumer should ack the message or not.
        /// </summary>
        public bool Ack { get; set; }

        public override string BindingKey => "amqp";

        protected override FixedFieldMap<AMQPOperationBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "expiration", (a, n) => { a.Expiration = (uint)n.GetIntegerValue(); } },
            { "userId", (a, n) => { a.UserId = n.GetScalarValueOrDefault(); } },
            { "cc", (a, n) => { a.Cc = n.CreateSimpleList(s => s.GetScalarValue()); } },
            { "priority", (a, n) => { a.Priority = n.GetIntegerValue(); } },
            { "deliveryMode", (a, n) => { a.DeliveryMode = (DeliveryMode)n.GetIntegerValue(); } },
            { "mandatory", (a, n) => { a.Mandatory = n.GetBooleanValue(); } },
            { "bcc", (a, n) => { a.Bcc = n.CreateSimpleList(s => s.GetScalarValue()); } },
            { "timestamp", (a, n) => { a.Timestamp = n.GetBooleanValue(); } },
            { "ack", (a, n) => { a.Ack = n.GetBooleanValue(); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeV3(writer);
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeProperties(writer);
        }

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteRequiredProperty<int>("expiration", (int)this.Expiration);
            writer.WriteRequiredProperty("userId", this.UserId);
            writer.WriteOptionalCollection("cc", this.Cc, (w, s) => w.WriteValue(s));
            writer.WriteRequiredProperty("priority", this.Priority);
            writer.WriteRequiredProperty("deliveryMode", (int)this.DeliveryMode);
            writer.WriteRequiredProperty("mandatory", this.Mandatory);
            writer.WriteOptionalCollection("bcc", this.Bcc, (w, s) => w.WriteValue(s));
            writer.WriteRequiredProperty("timestamp", this.Timestamp);
            writer.WriteRequiredProperty("ack", this.Ack);
            writer.WriteOptionalProperty(AsyncApiConstants.BindingVersion, this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
