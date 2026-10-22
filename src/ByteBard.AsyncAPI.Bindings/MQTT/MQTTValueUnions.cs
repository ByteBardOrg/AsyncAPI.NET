namespace ByteBard.AsyncAPI.Bindings.MQTT
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;
    using ByteBard.AsyncAPI.Writers;

    /// <summary>
    /// A MQTT binding value represented by either an integer (<see cref="MQTTInteger"/>) or a JSON Schema (<see cref="MQTTIntegerSchema"/>).
    /// </summary>
    /// <remarks>Integers and schemas convert implicitly, so <c>binding.SessionExpiryInterval = 600;</c> works.</remarks>
    public abstract class MQTTIntegerOrSchema
    {
        private protected MQTTIntegerOrSchema()
        {
        }

        public static implicit operator MQTTIntegerOrSchema(long value) => new MQTTInteger(value);

        public static implicit operator MQTTIntegerOrSchema(AsyncApiJsonSchema schema) => schema == null ? null : new MQTTIntegerSchema(schema);

        /// <summary>
        /// Parses a scalar as a signed 64-bit integer, or an object as a schema or schema reference.
        /// </summary>
        /// <returns>A <see cref="MQTTInteger"/> or <see cref="MQTTIntegerSchema"/>.</returns>
        /// <remarks>Schema field errors handled by the reader are recorded as diagnostics.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException">The scalar cannot be parsed as an Int64, or a non-scalar node is not a schema object.</exception>
        /// <exception cref="ByteBard.AsyncAPI.Exceptions.AsyncApiException">A schema reference has a non-scalar $ref value.</exception>
        public static MQTTIntegerOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? new MQTTInteger(node.GetLongValue())
                : new MQTTIntegerSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

        /// <summary>
        /// Writes the scalar value or schema directly, without a property name or wrapper object.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        public abstract void Serialize(IAsyncApiWriter writer, AsyncApiVersion version);
    }

    /// <summary>The integer alternative of <see cref="MQTTIntegerOrSchema"/>.</summary>
    /// <remarks>Holds the full 64-bit value, so MQTT values up to 4294967295 are kept. MQTT field limits are not enforced.</remarks>
    public sealed class MQTTInteger : MQTTIntegerOrSchema
    {
        public MQTTInteger(long value)
        {
            this.Value = value;
        }

        public long Value { get; }

        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static implicit operator long(MQTTInteger value) => (value ?? throw new ArgumentNullException(nameof(value))).Value;

        public override void Serialize(IAsyncApiWriter writer, AsyncApiVersion version) => writer.WriteValue(this.Value);
    }

    /// <summary>The schema alternative of <see cref="MQTTIntegerOrSchema"/>.</summary>
    public sealed class MQTTIntegerSchema : MQTTIntegerOrSchema
    {
        /// <summary>
        /// Retains the supplied schema or reference without copying it.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is null.</exception>
        public MQTTIntegerSchema(AsyncApiJsonSchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public AsyncApiJsonSchema Schema { get; }

        public static implicit operator AsyncApiJsonSchema(MQTTIntegerSchema value) => value?.Schema;

        public override void Serialize(IAsyncApiWriter writer, AsyncApiVersion version)
        {
            if (version == AsyncApiVersion.AsyncApi2_0) this.Schema.SerializeV2(writer);
            else this.Schema.SerializeV3(writer);
        }
    }

    /// <summary>
    /// A MQTT message response topic represented by either a string (<see cref="MQTTString"/>) or a JSON Schema (<see cref="MQTTStringSchema"/>).
    /// </summary>
    /// <remarks>Strings and schemas convert implicitly, so <c>binding.ResponseTopic = "replies";</c> works.</remarks>
    public abstract class MQTTStringOrSchema
    {
        private protected MQTTStringOrSchema()
        {
        }

        public static implicit operator MQTTStringOrSchema(string value) => value == null ? null : new MQTTString(value);

        public static implicit operator MQTTStringOrSchema(AsyncApiJsonSchema schema) => schema == null ? null : new MQTTStringSchema(schema);

        /// <summary>
        /// Converts a scalar to a response topic string, or parses an object as a schema or schema reference.
        /// </summary>
        /// <returns>A <see cref="MQTTString"/> or <see cref="MQTTStringSchema"/>.</returns>
        /// <remarks>Schema field errors handled by the reader are recorded as diagnostics.</remarks>
        /// <exception cref="ByteBard.AsyncAPI.Readers.Exceptions.AsyncApiReaderException">A non-scalar node is not a schema object.</exception>
        /// <exception cref="ByteBard.AsyncAPI.Exceptions.AsyncApiException">A schema reference has a non-scalar $ref value.</exception>
        public static MQTTStringOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? new MQTTString(node.GetScalarValue())
                : new MQTTStringSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

        /// <summary>
        /// Writes the scalar value or schema directly, without a property name or wrapper object.
        /// </summary>
        /// <param name="writer">The writer receiving the value.</param>
        /// <param name="version">Uses AsyncAPI 2.0 schema serialization for <see cref="AsyncApiVersion.AsyncApi2_0"/>; otherwise uses AsyncAPI 3.0.</param>
        public abstract void Serialize(IAsyncApiWriter writer, AsyncApiVersion version);
    }

    /// <summary>The string alternative of <see cref="MQTTStringOrSchema"/>.</summary>
    public sealed class MQTTString : MQTTStringOrSchema
    {
        /// <summary>
        /// Creates a response topic without validating topic syntax.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public MQTTString(string value)
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }

        public static implicit operator string(MQTTString value) => value?.Value;

        public override void Serialize(IAsyncApiWriter writer, AsyncApiVersion version) => writer.WriteValue(this.Value);
    }

    /// <summary>The schema alternative of <see cref="MQTTStringOrSchema"/>.</summary>
    public sealed class MQTTStringSchema : MQTTStringOrSchema
    {
        /// <summary>
        /// Retains the supplied schema or reference without copying it.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is null.</exception>
        public MQTTStringSchema(AsyncApiJsonSchema schema)
        {
            this.Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public AsyncApiJsonSchema Schema { get; }

        public static implicit operator AsyncApiJsonSchema(MQTTStringSchema value) => value?.Schema;

        public override void Serialize(IAsyncApiWriter writer, AsyncApiVersion version)
        {
            if (version == AsyncApiVersion.AsyncApi2_0) this.Schema.SerializeV2(writer);
            else this.Schema.SerializeV3(writer);
        }
    }
}
