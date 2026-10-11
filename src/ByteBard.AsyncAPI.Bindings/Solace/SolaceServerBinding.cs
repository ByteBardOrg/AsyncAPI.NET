namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding class for Solace servers.</summary>
    public class SolaceServerBinding : ServerBinding<SolaceServerBinding>
    {
        public string MsgVpn { get; set; }
        public string ClientName { get; set; }
        public override string BindingKey => "solace";

        protected override FixedFieldMap<SolaceServerBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => a.BindingVersion = n.GetScalarValue() },
            { "msgVpn", (a, n) => a.MsgVpn = n.GetScalarValue() },
            { "clientName", (a, n) => a.ClientName = n.GetScalarValue() },
        };

        public override void SerializeV2(IAsyncApiWriter writer) => this.SerializeProperties(writer);
        public override void SerializeV3(IAsyncApiWriter writer) => this.SerializeProperties(writer);

        public override void SerializeProperties(IAsyncApiWriter writer)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStartObject();
            writer.WriteOptionalProperty("msgVpn", this.MsgVpn);
            writer.WriteOptionalProperty("clientName", this.ClientName);
            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
