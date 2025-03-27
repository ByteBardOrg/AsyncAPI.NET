namespace ByteBard.AsyncAPI.Readers.Interface
{
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal interface IAsyncApiVersionService
    {
        AsyncApiReference ConvertToAsyncApiReference(string reference, ReferenceType? type);

        T LoadElement<T>(ParseNode node)
            where T : IAsyncApiElement;

        AsyncApiDocument LoadDocument(RootNode rootNode);
    }
}