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

        public void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("name", this.Name);
            writer.WriteOptionalObject("value", this.Value, (w, v) => w.WriteAny(v));
            writer.WriteEndObject();
        }
    }
}
