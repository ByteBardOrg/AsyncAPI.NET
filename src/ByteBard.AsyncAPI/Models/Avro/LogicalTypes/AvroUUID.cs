namespace ByteBard.AsyncAPI.Models.Avro.LogicalTypes
{
    public class AvroUUID : AvroLogicalType
    {
        public AvroUUID()
            : base(AvroPrimitiveType.String)
        {
        }

        public override LogicalType LogicalType => LogicalType.UUID;
    }
}
