namespace ByteBard.AsyncAPI.Models.Interfaces
{
    public interface IAsyncApiReferenceable : IAsyncApiSerializable
    {
        /// <summary>
        /// Indicates if object is populated with data or is just a reference to the data.
        /// </summary>
        bool UnresolvedReference { get; }

        /// <summary>
        /// Reference object.
        /// </summary>
        AsyncApiReference Reference { get; set; }
    }
}