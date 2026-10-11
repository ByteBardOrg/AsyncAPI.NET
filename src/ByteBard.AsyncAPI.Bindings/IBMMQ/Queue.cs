namespace ByteBard.AsyncAPI.Bindings.IBMMQ
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class Queue : IAsyncApiElement
    {
        public string ObjectName { get; set; }
        public bool? IsPartitioned { get; set; }
        public bool? Exclusive { get; set; }

        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("objectName", this.ObjectName);
            writer.WriteOptionalProperty("isPartitioned", this.IsPartitioned);
            writer.WriteOptionalProperty("exclusive", this.Exclusive);
            writer.WriteEndObject();
        }
    }
}
