namespace ByteBard.AsyncAPI.Bindings.GooglePubSub
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class SchemaSettings : IAsyncApiElement
    {
        public string Encoding { get; set; }
        public string FirstRevisionId { get; set; }
        public string LastRevisionId { get; set; }
        public string Name { get; set; }

        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("encoding", this.Encoding);
            writer.WriteOptionalProperty("firstRevisionId", this.FirstRevisionId);
            writer.WriteOptionalProperty("lastRevisionId", this.LastRevisionId);
            writer.WriteOptionalProperty("name", this.Name);
            writer.WriteEndObject();
        }
    }
}
