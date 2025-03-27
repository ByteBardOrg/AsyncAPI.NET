namespace ByteBard.AsyncAPI.Models.Avro.LogicalTypes
{
    public class AvroTimeMillis : AvroLogicalType
    {
        public AvroTimeMillis()
            : base(AvroPrimitiveType.Int)
        {
        }

        public override LogicalType LogicalType => LogicalType.Time_Millis;
    }
}
