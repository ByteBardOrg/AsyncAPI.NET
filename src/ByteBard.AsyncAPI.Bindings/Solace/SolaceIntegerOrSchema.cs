namespace ByteBard.AsyncAPI.Bindings.Solace
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// A Solace value represented by either an integer (<see cref="SolaceInteger"/>) or a JSON Schema or schema reference (<see cref="SolaceIntegerSchema"/>).
    /// </summary>
    /// <remarks>Integers and schemas convert implicitly, so <c>binding.Priority = 7;</c> works.</remarks>
    public abstract class SolaceIntegerOrSchema : IAsyncApiSerializable
    {
        private protected SolaceIntegerOrSchema()
        {
        }

        public static implicit operator SolaceIntegerOrSchema(long value) => new SolaceInteger(value);

        public static implicit operator SolaceIntegerOrSchema(AsyncApiJsonSchema schema) => schema == null ? null : new SolaceIntegerSchema(schema);

        /// <summary>
        /// Parses a scalar as a signed 64-bit integer, or an object as a schema or schema reference.
        /// </summary>
        /// <returns>A <see cref="SolaceInteger"/> or <see cref="SolaceIntegerSchema"/>.</returns>
        /// <remarks>Schema field errors handled by the reader are recorded as diagnostics. Scalar parsing errors propagate to the caller.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException">The scalar cannot be parsed as an Int64, or a non-scalar node is not a schema object.</exception>
        /// <exception cref="ByteBard.AsyncAPI.Exceptions.AsyncApiException">A schema reference has a non-scalar $ref value.</exception>
        public static SolaceIntegerOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? new SolaceInteger(node.GetLongValue())
                : new SolaceIntegerSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

        /// <summary>
        /// Writes the integer or schema value directly for AsyncAPI 2.0, without a property name or wrapper object.
        /// </summary>
        public abstract void SerializeV2(IAsyncApiWriter writer);

        /// <summary>
        /// Writes the integer or schema value directly for AsyncAPI 3.0, without a property name or wrapper object.
        /// </summary>
        public abstract void SerializeV3(IAsyncApiWriter writer);
    }

    /// <summary>The integer alternative of <see cref="SolaceIntegerOrSchema"/>.</summary>
    public sealed class SolaceInteger : SolaceIntegerOrSchema
    {
        public SolaceInteger(long value)
        {
            this.Value = value;
        }

        public long Value { get; }

        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static implicit operator long(SolaceInteger value) => (value ?? throw new ArgumentNullException(nameof(value))).Value;

        public override void SerializeV2(IAsyncApiWriter writer) => writer.WriteValue(this.Value);

        public override void SerializeV3(IAsyncApiWriter writer) => writer.WriteValue(this.Value);
    }

    /// <summary>The schema alternative of <see cref="SolaceIntegerOrSchema"/>.</summary>
    public sealed class SolaceIntegerSchema : SolaceIntegerOrSchema
    {
        /// <summary>
        /// Retains the supplied schema or reference without copying it.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is null.</exception>
        public SolaceIntegerSchema(AsyncApiJsonSchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public AsyncApiJsonSchema Schema { get; }

        public static implicit operator AsyncApiJsonSchema(SolaceIntegerSchema value) => value?.Schema;

        public override void SerializeV2(IAsyncApiWriter writer) => this.Schema.SerializeV2(writer);

        public override void SerializeV3(IAsyncApiWriter writer) => this.Schema.SerializeV3(writer);
    }
}
