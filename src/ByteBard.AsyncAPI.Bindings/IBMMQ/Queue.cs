namespace ByteBard.AsyncAPI.Bindings.IBMMQ
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class Queue : IAsyncApiElement
    {
        public string ObjectName { get; set; }
        public bool? IsPartitioned { get; set; }
        public bool? Exclusive { get; set; }

        /// <summary>
        /// Writes the IBM MQ queue settings object, omitting unset values.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("objectName", this.ObjectName);
            writer.WriteOptionalProperty<bool>("isPartitioned", this.IsPartitioned);
            writer.WriteOptionalProperty<bool>("exclusive", this.Exclusive);
            writer.WriteEndObject();
        }
    }
}
