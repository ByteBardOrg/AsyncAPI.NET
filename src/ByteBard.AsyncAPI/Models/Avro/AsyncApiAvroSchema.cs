namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public abstract class AsyncApiAvroSchema : IAsyncApiSerializable, IAsyncApiSchema
    {
        public abstract string Type { get; }

        /// <summary>
        /// A map of properties not in the schema, but added as additional metadata.
        /// </summary>
        public abstract IDictionary<string, AsyncApiAny> Metadata { get; set; }

        public static implicit operator AsyncApiAvroSchema(AvroPrimitiveType type)
        {
            return new AvroPrimitive(type);
        }

        public static explicit operator AvroPrimitiveType(AsyncApiAvroSchema schema)
        {
            if (schema is AvroPrimitive primitive)
            {
                return primitive.Type switch
                {
                    "null" => AvroPrimitiveType.Null,
                    "boolean" => AvroPrimitiveType.Boolean,
                    "int" => AvroPrimitiveType.Int,
                    "long" => AvroPrimitiveType.Long,
                    "float" => AvroPrimitiveType.Float,
                    "double" => AvroPrimitiveType.Double,
                    "bytes" => AvroPrimitiveType.Bytes,
                    "string" => AvroPrimitiveType.String,
                    _ => throw new InvalidCastException($"Avro schema type '{primitive.Type}' is not a primitive type."),
                };
            }

            throw new InvalidCastException($"Avro schema type '{schema?.Type}' is not a primitive type.");
        }

        public abstract void SerializeV2(IAsyncApiWriter writer);

        public abstract void SerializeV3(IAsyncApiWriter writer);

        public virtual bool TryGetAs<T>(out T result)
            where T : AsyncApiAvroSchema
        {
            result = this as T;
            return result != null;
        }

        public virtual bool Is<T>()
            where T : AsyncApiAvroSchema
        {
            return this is T;
        }

        public virtual T As<T>()
            where T : AsyncApiAvroSchema
        {
            return this as T;
        }
    }
}
