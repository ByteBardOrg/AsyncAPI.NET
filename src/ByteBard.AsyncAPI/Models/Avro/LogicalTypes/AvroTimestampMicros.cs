namespace ByteBard.AsyncAPI.Models.Avro.LogicalTypes
{
    public class AvroTimestampMicros : AvroLogicalType
    {
        public AvroTimestampMicros()
            : base(AvroPrimitiveType.Long)
        {
        }

        public override LogicalType LogicalType => LogicalType.Timestamp_Micros;
    }
}
