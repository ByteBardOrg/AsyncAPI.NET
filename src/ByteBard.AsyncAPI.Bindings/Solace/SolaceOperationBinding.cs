namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>Binding class for Solace operations.</summary>
    public class SolaceOperationBinding : OperationBinding<SolaceOperationBinding>
    {
        public List<SolaceDestination> Destinations { get; set; } = new();
        public SolaceIntegerOrSchema TimeToLive { get; set; }
        public SolaceIntegerOrSchema Priority { get; set; }
        public bool? DmqEligible { get; set; }
        public override string BindingKey => "solace";

        protected override FixedFieldMap<SolaceOperationBinding> FixedFieldMap => new()
        {
            { "bindingVersion", (a, n) => a.BindingVersion = n.GetScalarValue() },
            { "destinations", (a, n) => a.Destinations = n.CreateList(s => s.ParseMap(SolaceDestination.Fields)) },
            { "timeToLive", (a, n) => a.TimeToLive = ReadIntegerOrSchema(n) },
            { "priority", (a, n) => a.Priority = ReadIntegerOrSchema(n) },
            { "dmqEligible", (a, n) => a.DmqEligible = n.GetBooleanValue() },
        };

        internal static SolaceIntegerOrSchema ReadIntegerOrSchema(ParseNode node)
        {
            if (node is ValueNode)
            {
                return new SolaceIntegerOrSchema { Integer = node.GetLongValue() };
            }

            return new SolaceIntegerOrSchema { Schema = AsyncApiJsonSchemaDeserializer.LoadSchema(node) };
        }

        public override void SerializeV2(IAsyncApiWriter writer) => Serialize(writer, false);
        public override void SerializeV3(IAsyncApiWriter writer) => Serialize(writer, true);

        public override void SerializeProperties(IAsyncApiWriter writer) => Serialize(writer, false);

        private void Serialize(IAsyncApiWriter writer, bool v3)
        {
            if (writer is null) throw new ArgumentNullException(nameof(writer));
            this.TimeToLive?.Validate();
            this.Priority?.Validate();
            writer.WriteStartObject();
            writer.WriteOptionalCollection("destinations", this.Destinations, (w, d) => d.Serialize(w));
            writer.WriteOptionalObject("timeToLive", this.TimeToLive, (w, value) => { if (v3) value.SerializeV3(w); else value.SerializeV2(w); });
            writer.WriteOptionalObject("priority", this.Priority, (w, value) => { if (v3) value.SerializeV3(w); else value.SerializeV2(w); });
            if (this.DmqEligible.HasValue)
            {
                writer.WritePropertyName("dmqEligible");
                writer.WriteValue(this.DmqEligible.Value);
            }

            writer.WriteOptionalProperty("bindingVersion", this.BindingVersion);
            writer.WriteExtensions(this.Extensions);
            writer.WriteEndObject();
        }
    }
}
