namespace ByteBard.AsyncAPI.Bindings.MQTT
{
    using System;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class LastWill : IAsyncApiElement
    {
        private bool? retain;

        /// <summary>
        /// The topic where the Last Will and Testament message will be sent.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Defines how hard the broker/client will try to ensure that
        /// the Last Will and Testament message is received.
        /// Its value MUST be either 0, 1 or 2.
        /// </summary>
        public uint? QoS { get; set; }

        /// <summary>
        /// Last Will message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Whether the broker should retain the Last Will and Testament message or not.
        /// </summary>
        public bool Retain
        {
            get => this.retain ?? false;
            set => this.retain = value;
        }

        public void Serialize(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            writer.WriteOptionalProperty("topic", this.Topic);
            writer.WriteOptionalProperty("qos", (int?)this.QoS);
            writer.WriteOptionalProperty("message", this.Message);
            writer.WriteOptionalProperty("retain", this.retain);
            writer.WriteEndObject();
        }
    }
}