namespace ByteBard.AsyncAPI.Bindings.IBMMQ
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class Topic : IAsyncApiElement
    {
        public string StringValue { get; set; }
        public string ObjectName { get; set; }
        public bool? DurablePermitted { get; set; }
        public bool? LastMsgRetained { get; set; }

        /// <summary>
        /// Writes the IBM MQ topic settings object, omitting unset values.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("string", this.StringValue);
            writer.WriteOptionalProperty("objectName", this.ObjectName);
            writer.WriteOptionalProperty("durablePermitted", this.DurablePermitted);
            writer.WriteOptionalProperty("lastMsgRetained", this.LastMsgRetained);
            writer.WriteEndObject();
        }
    }
}
