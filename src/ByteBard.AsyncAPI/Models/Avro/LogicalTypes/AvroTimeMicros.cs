namespace ByteBard.AsyncAPI.Models.Avro.LogicalTypes
{
    public class AvroTimeMicros : AvroLogicalType
    {
        public AvroTimeMicros()
            : base(AvroPrimitiveType.Long)
        {
        }

        public override LogicalType LogicalType => LogicalType.Time_Micros;
    }
}
