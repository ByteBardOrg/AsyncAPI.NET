namespace ByteBard.AsyncAPI.Bindings.IBMMQ
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    public class IBMMQChannelBinding : ChannelBinding<IBMMQChannelBinding>
    {
        public string DestinationType { get; set; }
        public Queue Queue { get; set; }
        public Topic Topic { get; set; }
        public int? MaxMsgLength { get; set; }

        public override string BindingKey => "ibmmq";

        protected override FixedFieldMap<IBMMQChannelBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "destinationType", (a, n) => { a.DestinationType = n.GetScalarValue(); } },
            { "queue", (a, n) => { a.Queue = n.ParseMap(queueFields); } },
            { "topic", (a, n) => { a.Topic = n.ParseMap(topicFields); } },
            { "maxMsgLength", (a, n) => { a.MaxMsgLength = n.GetIntegerValue(); } },
        };

        private static readonly FixedFieldMap<Queue> queueFields = new()
        {
            { "objectName", (a, n) => { a.ObjectName = n.GetScalarValue(); } },
            { "isPartitioned", (a, n) => { a.IsPartitioned = n.GetBooleanValue(); } },
            { "exclusive", (a, n) => { a.Exclusive = n.GetBooleanValue(); } },
        };

        private static readonly FixedFieldMap<Topic> topicFields = new()
        {
            { "string", (a, n) => { a.StringValue = n.GetScalarValue(); } },
            { "objectName", (a, n) => { a.ObjectName = n.GetScalarValue(); } },
            { "durablePermitted", (a, n) => { a.DurablePermitted = n.GetBooleanValue(); } },
            { "lastMsgRetained", (a, n) => { a.LastMsgRetained = n.GetBooleanValue(); } },
        };

        /// <summary>
        /// Writes the IBM MQ channel binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);
        /// <summary>
        /// Writes the IBM MQ channel binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        /// <summary>
        /// Writes the IBM MQ channel binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <remarks>An unset binding version is omitted.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("destinationType", this.DestinationType);
            writer.WriteOptionalObject("queue", this.Queue, (w, v) => v.Serialize(w));
            writer.WriteOptionalObject("topic", this.Topic, (w, v) => v.Serialize(w));
            writer.WriteOptionalProperty("maxMsgLength", this.MaxMsgLength);
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
