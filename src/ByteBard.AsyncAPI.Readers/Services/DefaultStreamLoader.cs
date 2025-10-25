namespace ByteBard.AsyncAPI.Readers.Services
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ByteBard.AsyncAPI.Readers.Exceptions;
    using ByteBard.AsyncAPI.Readers.Interface;

    internal class DefaultStreamLoader : IStreamLoader
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public Stream Load(Uri uri)
        {
            try
            {
                if (uri.IsAbsoluteUri)
                {
                    switch (uri.Scheme.ToLowerInvariant())
                    {
                        case "file":
                            return File.OpenRead(uri.LocalPath);
                        case "http":
                        case "https":
                            return HttpClient.GetStreamAsync(uri).GetAwaiter().GetResult();
                        default:
                            throw new ArgumentException("Unsupported scheme");
                    }
                }
                else
                {
                    return File.OpenRead(uri.OriginalString);
                }
            }
            catch (Exception ex)
            {
                throw new AsyncApiReaderException($"Something went wrong trying to fetch '{uri.OriginalString}'. {ex.Message}", ex);
            }
        }

        public async Task<Stream> LoadAsync(Uri uri)
        {
            try
            {
                if (uri.IsAbsoluteUri)
                {
                    switch (uri.Scheme.ToLowerInvariant())
                    {
                        case "file":
                            return File.OpenRead(uri.LocalPath);
                        case "http":
                        case "https":
                            return await HttpClient.GetStreamAsync(uri);
                        default:
                            throw new ArgumentException("Unsupported scheme");
                    }
                }
                else
                {
                    return File.OpenRead(uri.OriginalString);
                }
            }
            catch (Exception ex)
            {
                throw new AsyncApiReaderException($"Something went wrong trying to fetch '{uri.OriginalString}'. {ex.Message}", ex);
            }
        }
    }
}
