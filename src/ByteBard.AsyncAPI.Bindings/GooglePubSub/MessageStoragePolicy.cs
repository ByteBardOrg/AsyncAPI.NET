namespace ByteBard.AsyncAPI.Bindings.GooglePubSub
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public class MessageStoragePolicy : IAsyncApiElement
    {
        public IEnumerable<string> AllowedPersistenceRegions { get; set; }

        /// <summary>
        /// Writes the message storage policy object, omitting null or empty persistence regions.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalCollection("allowedPersistenceRegions", this.AllowedPersistenceRegions, (w, v) => w.WriteValue(v));
            writer.WriteEndObject();
        }
    }
}
