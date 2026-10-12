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

        /// <summary>
        /// Parses a scalar as a signed 64-bit integer, or an object as a schema or schema reference.
        /// </summary>
        /// <returns>A value containing the parsed integer or schema.</returns>
        /// <remarks>Schema field errors handled by the reader are recorded as diagnostics. Scalar parsing errors propagate to the caller.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException">The scalar cannot be parsed as an Int64, or a non-scalar node is not a schema object.</exception>
        /// <exception cref="ByteBard.AsyncAPI.Exceptions.AsyncApiException">A schema reference has a non-scalar $ref value.</exception>
        internal static SolaceIntegerOrSchema ReadIntegerOrSchema(ParseNode node)
        {
            if (node is ValueNode)
            {
                return new SolaceIntegerOrSchema { Integer = node.GetLongValue() };
            }

            return new SolaceIntegerOrSchema { Schema = AsyncApiJsonSchemaDeserializer.LoadSchema(node) };
        }

        /// <summary>
        /// Writes the Solace operation binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">TimeToLive or Priority is present but contains neither an integer nor a schema.</exception>
        public override void SerializeV2(IAsyncApiWriter writer) => Serialize(writer, false);
        /// <summary>
        /// Writes the Solace operation binding object and its extensions for AsyncAPI 3.0.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">TimeToLive or Priority is present but contains neither an integer nor a schema.</exception>
        public override void SerializeV3(IAsyncApiWriter writer) => Serialize(writer, true);

        /// <summary>
        /// Writes the Solace operation binding object and its extensions for AsyncAPI 2.0.
        /// </summary>
        /// <remarks>Validates the optional integer-or-schema values before writing the binding. Explicit false DmqEligible values are preserved.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">TimeToLive or Priority is present but contains neither an integer nor a schema.</exception>
        public override void SerializeProperties(IAsyncApiWriter writer) => Serialize(writer, false);

        /// <summary>
        /// Validates the optional integer-or-schema values, then writes the binding object and its extensions.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="v3">Uses AsyncAPI 3.0 schema serialization when true and AsyncAPI 2.0 when false.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        /// <exception cref="InvalidOperationException">TimeToLive or Priority is present but contains neither an integer nor a schema.</exception>
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
