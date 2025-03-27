namespace ByteBard.AsyncAPI.Services
{
    using ByteBard.AsyncAPI.Exceptions;
    using ByteBard.AsyncAPI.Models;

    internal class AsyncApiReferenceError : AsyncApiError
    {
        public AsyncApiReferenceError(AsyncApiException exception)
            : base(exception)
        {
        }
    }
}