namespace ByteBard.AsyncAPI.Models.Avro.LogicalTypes
{
    public class AvroDate : AvroLogicalType
    {
        public AvroDate()
            : base(AvroPrimitiveType.Int)
        {
        }

        public override LogicalType LogicalType => LogicalType.Date;
    }
}
