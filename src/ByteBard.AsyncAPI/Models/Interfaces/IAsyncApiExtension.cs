namespace ByteBard.AsyncAPI.Models.Interfaces
{
    using ByteBard.AsyncAPI.Writers;

    public interface IAsyncApiExtension
    {
        void Write(IAsyncApiWriter writer);
    }
}