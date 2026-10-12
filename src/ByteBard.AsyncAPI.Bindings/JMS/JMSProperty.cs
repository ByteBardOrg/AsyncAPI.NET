namespace ByteBard.AsyncAPI.Bindings.JMS
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>A name and typed value for a JMS connection property.</summary>
    public class JMSProperty : IAsyncApiElement
    {
        public string Name { get; set; }

        public AsyncApiAny Value { get; set; }

        /// <summary>
        /// Writes the connection property object, preserving the JSON type of its value and omitting unset fields.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("name", this.Name);
            writer.WriteOptionalObject("value", this.Value, (w, v) => w.WriteAny(v));
            writer.WriteEndObject();
        }
    }
}
