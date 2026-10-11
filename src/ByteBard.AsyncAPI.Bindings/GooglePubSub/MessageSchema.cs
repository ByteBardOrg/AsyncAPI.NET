namespace ByteBard.AsyncAPI.Bindings.GooglePubSub
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class MessageSchema : IAsyncApiElement
    {
        public string Name { get; set; }

        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("name", this.Name);
            writer.WriteEndObject();
        }
    }
}
