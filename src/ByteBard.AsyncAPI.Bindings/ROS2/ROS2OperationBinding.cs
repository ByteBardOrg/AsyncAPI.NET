namespace ByteBard.AsyncAPI.Bindings.ROS2
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding class for ROS 2 operation settings.</summary>
    public class ROS2OperationBinding : OperationBinding<ROS2OperationBinding>
    {
        public string Role { get; set; }
        public string Node { get; set; }
        public ROS2QosPolicies QosPolicies { get; set; }

        public override string BindingKey => "ros2";

        protected override FixedFieldMap<ROS2OperationBinding> FixedFieldMap => new()
        {
            { "role", (a, n) => { a.Role = n.GetScalarValue(); } },
            { "node", (a, n) => { a.Node = n.GetScalarValue(); } },
            { "qosPolicies", (a, n) => { a.QosPolicies = n.ParseMap(ROS2QosPolicies.FixedFields); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("role", this.Role);
            writer.WriteOptionalProperty("node", this.Node);
            writer.WriteOptionalObject("qosPolicies", this.QosPolicies, (w, qos) => qos.Serialize(w));
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
