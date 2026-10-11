namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Queue destination settings in a Solace operation binding.</summary>
    public class SolaceQueue : IAsyncApiElement
    {
        public string Name { get; set; }
        public List<string> TopicSubscriptions { get; set; } = new();
        public string AccessType { get; set; }
        public string MaxMsgSpoolSize { get; set; }
        public string MaxTtl { get; set; }

        internal static FixedFieldMap<SolaceQueue> Fields => new()
        {
            { "name", (a, n) => a.Name = n.GetScalarValue() },
            { "topicSubscriptions", (a, n) => a.TopicSubscriptions = n.CreateSimpleList(s => s.GetScalarValue()) },
            { "accessType", (a, n) => a.AccessType = n.GetScalarValue() },
            { "maxMsgSpoolSize", (a, n) => a.MaxMsgSpoolSize = n.GetScalarValue() },
            { "maxTtl", (a, n) => a.MaxTtl = n.GetScalarValue() },
        };

        /// <summary>
        /// Writes the queue settings object, omitting unset values and empty topic subscriptions.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public void Serialize(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("name", this.Name);
            writer.WriteOptionalCollection("topicSubscriptions", this.TopicSubscriptions, (w, s) => w.WriteValue(s));
            writer.WriteOptionalProperty("accessType", this.AccessType);
            writer.WriteOptionalProperty("maxMsgSpoolSize", this.MaxMsgSpoolSize);
            writer.WriteOptionalProperty("maxTtl", this.MaxTtl);
            writer.WriteEndObject();
        }
    }
}
