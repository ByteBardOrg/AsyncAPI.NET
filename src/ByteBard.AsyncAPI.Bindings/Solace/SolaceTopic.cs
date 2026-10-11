namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Topic destination settings in a Solace operation binding.</summary>
    public class SolaceTopic : IAsyncApiElement
    {
        public List<string> TopicSubscriptions { get; set; } = new();

        internal static FixedFieldMap<SolaceTopic> Fields => new()
        {
            { "topicSubscriptions", (a, n) => a.TopicSubscriptions = n.CreateSimpleList(s => s.GetScalarValue()) },
        };

        public void Serialize(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalCollection("topicSubscriptions", this.TopicSubscriptions, (w, s) => w.WriteValue(s));
            writer.WriteEndObject();
        }
    }
}
