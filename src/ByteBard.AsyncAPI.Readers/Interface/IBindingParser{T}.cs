namespace ByteBard.AsyncAPI.Readers.Interface
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public interface IBindingParser<out T> : IBinding
    {
        T LoadBinding(PropertyNode node);
    }
}
