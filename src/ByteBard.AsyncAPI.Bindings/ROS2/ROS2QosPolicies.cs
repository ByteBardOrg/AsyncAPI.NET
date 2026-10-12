namespace ByteBard.AsyncAPI.Bindings.ROS2
{
    using System;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Quality-of-service policies for a ROS 2 operation.</summary>
    public class ROS2QosPolicies : IAsyncApiElement
    {
        public string Reliability { get; set; }
        public string History { get; set; }
        public string Durability { get; set; }
        public int? Lifespan { get; set; }
        public int? Deadline { get; set; }
        public string Liveliness { get; set; }
        public int? LeaseDuration { get; set; }

        internal static FixedFieldMap<ROS2QosPolicies> FixedFields => new()
        {
            { "reliability", (a, n) => { a.Reliability = n.GetScalarValue(); } },
            { "history", (a, n) => { a.History = n.GetScalarValue(); } },
            { "durability", (a, n) => { a.Durability = n.GetScalarValue(); } },
            { "lifespan", (a, n) => { a.Lifespan = n.GetIntegerValueOrDefault(); } },
            { "deadline", (a, n) => { a.Deadline = n.GetIntegerValueOrDefault(); } },
            { "liveliness", (a, n) => { a.Liveliness = n.GetScalarValue(); } },
            { "leaseDuration", (a, n) => { a.LeaseDuration = n.GetIntegerValueOrDefault(); } },
        };

        /// <summary>
        /// Writes the QoS policy object, omitting unset values and preserving zero and negative numeric values.
        /// </summary>
        /// <exception cref="System.NullReferenceException"><paramref name="writer"/> is null.</exception>
        internal void Serialize(IAsyncApiWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("reliability", this.Reliability);
            writer.WriteOptionalProperty("history", this.History);
            writer.WriteOptionalProperty("durability", this.Durability);
            writer.WriteOptionalProperty("lifespan", this.Lifespan);
            writer.WriteOptionalProperty("deadline", this.Deadline);
            writer.WriteOptionalProperty("liveliness", this.Liveliness);
            writer.WriteOptionalProperty("leaseDuration", this.LeaseDuration);
            writer.WriteEndObject();
        }
    }
}
