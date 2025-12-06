namespace ByteBard.AsyncAPI.Readers.Interface
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IStreamLoader
    {
        Task<Stream> LoadAsync(Uri baseUri, Uri uri);

        Stream Load(Uri baseUri, Uri uri);
    }
}