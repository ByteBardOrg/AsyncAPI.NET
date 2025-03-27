namespace ByteBard.AsyncAPI.Readers.Exceptions
{
    using System;
    using ByteBard.AsyncAPI.Exceptions;

    [Serializable]
    public class AsyncApiReaderException : AsyncApiException
    {
        public AsyncApiReaderException()
        {
        }

        public AsyncApiReaderException(string message)
            : base(message)
        {
        }

        public AsyncApiReaderException(string message, ParsingContext context)
            : base(message)
        {
            this.Pointer = context.GetLocation();
        }

        public AsyncApiReaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
