namespace ByteBard.AsyncAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ByteBard.AsyncAPI.Writers;

    public class AvroMap : AsyncApiAvroSchema
    {
        public override string Type { get; } = "map";

        public AsyncApiAvroSchema Values { get; set; }

        /// <summary>
        /// A map of properties not in the schema, but added as additional metadata.
        /// </summary>
        public override IDictionary<string, AsyncApiAny> Metadata { get; set; } = new Dictionary<string, AsyncApiAny>();

        public override void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer, (w, s) => s.SerializeV2(w));
        }

        public override void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer, (w, s) => s.SerializeV3(w));
        }

        public void SerializeCore(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer, (w, s) => s.SerializeV2(w));
        }

        private void SerializeCore(IAsyncApiWriter writer, Action<IAsyncApiWriter, AsyncApiAvroSchema> action)
        {
            writer.WriteStartObject();
            writer.WriteOptionalProperty("type", this.Type);
            writer.WriteRequiredObject("values", this.Values, action);
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

            writer.WriteEndObject();
        }
    }
}
