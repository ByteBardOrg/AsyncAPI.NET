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

        /// <summary>
        /// Writes the schema settings object, omitting unset values.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
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
