namespace ByteBard.AsyncAPI.Bindings.GooglePubSub
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class MessageSchema : IAsyncApiElement
    {
        public string Name { get; set; }

        /// <summary>
        /// Writes the message schema object, omitting an unset name.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("name", this.Name);
            writer.WriteEndObject();
        }
    }
}
