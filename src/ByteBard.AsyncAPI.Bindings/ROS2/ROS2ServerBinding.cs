namespace ByteBard.AsyncAPI.Bindings.ROS2
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding class for ROS 2 server settings.</summary>
    public class ROS2ServerBinding : ServerBinding<ROS2ServerBinding>
    {
        /// <summary>The ROS middleware implementation.</summary>
        public string RmwImplementation { get; set; }

        /// <summary>The ROS domain ID.</summary>
        public int? DomainId { get; set; }

        public override string BindingKey => "ros2";

        protected override FixedFieldMap<ROS2ServerBinding> FixedFieldMap => new()
        {
            { "rmwImplementation", (a, n) => { a.RmwImplementation = n.GetScalarValue(); } },
            { "domainId", (a, n) => { a.DomainId = n.GetIntegerValueOrDefault(); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);

        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("rmwImplementation", this.RmwImplementation);
            writer.WriteOptionalProperty("domainId", this.DomainId);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
