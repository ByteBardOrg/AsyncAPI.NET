namespace ByteBard.AsyncAPI.Bindings.IBMMQ
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    public class IBMMQServerBinding : ServerBinding<IBMMQServerBinding>
    {
        public string GroupId { get; set; }
        public string CcdtQueueManagerName { get; set; }
        public string CipherSpec { get; set; }
        public bool? MultiEndpointServer { get; set; }
        public int? HeartBeatInterval { get; set; }

        public override string BindingKey => "ibmmq";

        protected override FixedFieldMap<IBMMQServerBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => { a.BindingVersion = n.GetScalarValue(); } },
            { "groupId", (a, n) => { a.GroupId = n.GetScalarValue(); } },
            { "ccdtQueueManagerName", (a, n) => { a.CcdtQueueManagerName = n.GetScalarValue(); } },
            { "cipherSpec", (a, n) => { a.CipherSpec = n.GetScalarValue(); } },
            { "multiEndpointServer", (a, n) => { a.MultiEndpointServer = n.GetBooleanValue(); } },
            { "heartBeatInterval", (a, n) => { a.HeartBeatInterval = n.GetIntegerValue(); } },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeV3(writer);
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("groupId", this.GroupId);
            writer.WriteOptionalProperty("ccdtQueueManagerName", this.CcdtQueueManagerName);
            writer.WriteOptionalProperty("cipherSpec", this.CipherSpec);
            writer.WriteOptionalProperty("multiEndpointServer", this.MultiEndpointServer);
            writer.WriteOptionalProperty("heartBeatInterval", this.HeartBeatInterval);
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
