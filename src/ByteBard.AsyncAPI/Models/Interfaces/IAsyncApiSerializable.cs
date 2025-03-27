namespace ByteBard.AsyncAPI.Models.Interfaces
{
    using ByteBard.AsyncAPI.Writers;

    public interface IAsyncApiSerializable : IAsyncApiElement
    {
        void SerializeV2(IAsyncApiWriter writer);
    }
}
