namespace LEGO.AsyncAPI.Extensions;

using LEGO.AsyncAPI.Models.Interfaces;
using LEGO.AsyncAPI.Writers;

public static class AsyncApiReferenceableExtensions
{
    public static string SerializeAsDollarRef(this IAsyncApiReferenceable reference)
    {
        return reference.Reference.Reference;
    }
}
