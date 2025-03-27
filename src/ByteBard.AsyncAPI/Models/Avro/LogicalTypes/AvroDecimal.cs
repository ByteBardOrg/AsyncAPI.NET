namespace ByteBard.AsyncAPI.Models.Avro.LogicalTypes
{
    using ByteBard.AsyncAPI.Writers;

    public class AvroDecimal : AvroLogicalType
    {
        public AvroDecimal()
            : base(AvroPrimitiveType.Bytes)
        {
        }

        public override LogicalType LogicalType => LogicalType.Decimal;

        public int? Scale { get; set; }

        public int? Precision { get; set; }

        public override void SerializeV2Core(IAsyncApiWriter writer)
        {
            writer.WriteOptionalProperty("scale", this.Scale);
            writer.WriteOptionalProperty("precision", this.Precision);
        }
    }
}
