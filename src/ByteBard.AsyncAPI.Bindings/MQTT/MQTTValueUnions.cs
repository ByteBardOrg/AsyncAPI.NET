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

        public static MQTTIntegerOrSchema FromInteger(int value) => new(value, null);

        public static MQTTIntegerOrSchema FromLong(long value) => new(value, null);

        public static MQTTIntegerOrSchema FromSchema(AsyncApiJsonSchema schema) => new(null, schema ?? throw new ArgumentNullException(nameof(schema)));

        public static MQTTIntegerOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? FromLong(node.GetLongValue())
                : FromSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

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
        private MQTTStringOrSchema(string value, AsyncApiJsonSchema schema)
        {
            this.Value = value;
            this.Schema = schema;
        }

        public string Value { get; }

        public AsyncApiJsonSchema Schema { get; }

        public static MQTTStringOrSchema FromString(string value) => new(value ?? throw new ArgumentNullException(nameof(value)), null);

        public static MQTTStringOrSchema FromSchema(AsyncApiJsonSchema schema) => new(null, schema ?? throw new ArgumentNullException(nameof(schema)));

        public static MQTTStringOrSchema Parse(ParseNode node)
        {
            return node is ValueNode
                ? FromString(node.GetScalarValue())
                : FromSchema(AsyncApiJsonSchemaDeserializer.LoadSchema(node));
        }

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
