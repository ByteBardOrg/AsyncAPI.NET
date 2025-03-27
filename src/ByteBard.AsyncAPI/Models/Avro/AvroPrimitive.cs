namespace ByteBard.AsyncAPI.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using ByteBard.AsyncAPI.Writers;

    public class AvroPrimitive : AsyncApiAvroSchema
    {
        public override string Type { get; }

        /// <summary>
        /// A map of properties not in the schema, but added as additional metadata.
        /// </summary>
        public override IDictionary<string, AsyncApiAny> Metadata { get; set; } = new Dictionary<string, AsyncApiAny>();

        public AvroPrimitive(AvroPrimitiveType type)
        {
            this.Type = type.GetDisplayName();
        }

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            writer.WriteValue(this.Type);
            if (this.Metadata.Any())
            {
                foreach (var item in this.Metadata)
                {
                    writer.WritePropertyName(item.Key);
                    if (item.Value == null)
                    {
                        writer.WriteNull();
                    }
                    else
                    {
                        writer.WriteAny(item.Value);
                    }
                }
            }
        }
    }
}