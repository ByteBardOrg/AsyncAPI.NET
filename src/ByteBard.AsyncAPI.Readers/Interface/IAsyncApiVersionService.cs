namespace ByteBard.AsyncAPI.Readers.Interface
{
    using System.Text.Json.Nodes;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    internal interface IAsyncApiVersionService
    {
        AsyncApiReference ConvertToAsyncApiReference(string reference, ReferenceType? type);

        T LoadElement<T>(ParseNode node)
            where T : IAsyncApiElement;

        AsyncApiDocument LoadDocument(RootNode rootNode);

        /// <summary>
        /// Adjusts the fragment a reference points at before it is loaded, for what the version keeps elsewhere.
        /// </summary>
        JsonNode PrepareReferenceFragment(JsonNode fragment, IAsyncApiReferenceable reference, ParsingContext context);
    }
}
