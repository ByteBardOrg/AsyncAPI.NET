namespace ByteBard.AsyncAPI.Bindings.MQTT
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>A MQTT binding value represented by either an integer or a JSON Schema.</summary>
    public sealed class MQTTIntegerOrSchema
    {
        /// <summary>
        /// Stores the supplied integer and schema without validating that exactly one is present.
        /// </summary>
        private MQTTIntegerOrSchema(long? integer, AsyncApiJsonSchema schema)
        {
            this.LongInteger = integer;
            this.Schema = schema;
        }

        /// <summary>The integer value when it is within the range of the legacy int API.</summary>
        public int? Integer => this.LongInteger.HasValue && this.LongInteger.Value >= int.MinValue && this.LongInteger.Value <= int.MaxValue
            ? (int)this.LongInteger.Value
            : null;

        /// <summary>The full-width integer value.</summary>
        public long? LongInteger { get; }

        public AsyncApiJsonSchema Schema { get; }

        /// <summary>
        /// Creates an integer alternative without enforcing MQTT field limits.
        /// </summary>
        public static MQTTIntegerOrSchema FromInteger(int value) => new(value, null);

        /// <summary>
        /// Creates an integer alternative without enforcing MQTT field limits.
        /// </summary>
        /// <remarks>Values outside the Int32 range remain available through LongInteger; Integer returns null for them.</remarks>
        public static MQTTIntegerOrSchema FromLong(long value) => new(value, null);

        /// <summary>
        /// Creates a schema alternative, retaining the supplied schema or reference without copying it.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is null.</exception>
        public static MQTTIntegerOrSchema FromSchema(AsyncApiJsonSchema schema) => new(null, schema ?? throw new ArgumentNullException(nameof(schema)));

        /// <summary>
        /// Parses a scalar as a signed 64-bit integer, or an object as a schema or schema reference.
        /// </summary>
        /// <returns>A value containing the parsed scalar or schema.</returns>
        /// <remarks>Schema field errors handled by the reader are recorded as diagnostics.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException">The scalar cannot be parsed as an Int64, or a non-scalar node is not a schema object.</exception>
        /// <exception cref="ByteBard.AsyncAPI.Exceptions.AsyncApiException">A schema reference has a non-scalar $ref value.</exception>
        public static MQTTIntegerOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? FromLong(node.GetLongValue())
                : FromSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

        /// <summary>
        /// Writes the scalar value or schema directly, without a property name or wrapper object.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        /// <exception cref="NullReferenceException">The writer is null when writing a scalar or an ordinary schema.</exception>
        /// <exception cref="ArgumentNullException">The writer is null when writing a schema reference.</exception>
        public void Serialize(IAsyncApiWriter writer, AsyncApiVersion version)
        {
            if (this.LongInteger.HasValue)
            {
                writer.WriteValue(this.LongInteger.Value);
            }
            else if (this.Schema != null)
            {
                if (version == AsyncApiVersion.AsyncApi2_0) this.Schema.SerializeV2(writer);
                else this.Schema.SerializeV3(writer);
            }
        }
    }

    /// <summary>A MQTT message response topic represented by either a string or a JSON Schema.</summary>
    public sealed class MQTTStringOrSchema
    {
        /// <summary>
        /// Stores the supplied response topic and schema without validating that exactly one is present.
        /// </summary>
        private MQTTStringOrSchema(string value, AsyncApiJsonSchema schema)
        {
            this.Value = value;
            this.Schema = schema;
        }

        public string Value { get; }

        public AsyncApiJsonSchema Schema { get; }

        /// <summary>
        /// Creates a response topic alternative without validating topic syntax.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static MQTTStringOrSchema FromString(string value) => new(value ?? throw new ArgumentNullException(nameof(value)), null);

        /// <summary>
        /// Creates a schema alternative, retaining the supplied schema or reference without copying it.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is null.</exception>
        public static MQTTStringOrSchema FromSchema(AsyncApiJsonSchema schema) => new(null, schema ?? throw new ArgumentNullException(nameof(schema)));

        /// <summary>
        /// Converts a scalar to a response topic string, or parses an object as a schema or schema reference.
        /// </summary>
        /// <returns>A value containing the parsed scalar or schema.</returns>
        /// <remarks>Schema field errors handled by the reader are recorded as diagnostics.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException">A non-scalar node is not a schema object.</exception>
        /// <exception cref="ByteBard.AsyncAPI.Exceptions.AsyncApiException">A schema reference has a non-scalar $ref value.</exception>
        public static MQTTStringOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? FromString(node.GetScalarValue())
                : FromSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

        /// <summary>
        /// Writes the scalar value or schema directly, without a property name or wrapper object.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        /// <exception cref="NullReferenceException">The writer is null when writing a scalar or an ordinary schema.</exception>
        /// <exception cref="ArgumentNullException">The writer is null when writing a schema reference.</exception>
        public void Serialize(IAsyncApiWriter writer, AsyncApiVersion version)
        {
            if (this.Schema != null)
            {
                if (version == AsyncApiVersion.AsyncApi2_0) this.Schema.SerializeV2(writer);
                else this.Schema.SerializeV3(writer);
            }
            else if (this.Value != null)
            {
                writer.WriteValue(this.Value);
            }
        }
    }
}
