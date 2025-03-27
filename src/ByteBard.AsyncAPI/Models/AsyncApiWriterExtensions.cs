namespace ByteBard.AsyncAPI.Models
{
    using ByteBard.AsyncAPI.Writers;

    public static class AsyncApiWriterExtensions
    {
        internal static AsyncApiWriterSettings GetSettings(this IAsyncApiWriter asyncApiWriter)
        {
            if (asyncApiWriter is AsyncApiWriterBase)
            {
                return ((AsyncApiWriterBase)asyncApiWriter).Settings;
            }

            return new AsyncApiWriterSettings();
        }
    }
}