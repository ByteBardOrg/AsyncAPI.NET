namespace ByteBard.AsyncAPI.Bindings
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers.Interface;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public abstract class Binding<T> : AsyncApiBinding, IBindingParser<T>
        where T : IBinding, new()
    {
        public abstract T LoadBinding(PropertyNode node);
    }
}
