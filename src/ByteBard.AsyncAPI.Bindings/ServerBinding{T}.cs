namespace ByteBard.AsyncAPI.Bindings
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public abstract class ServerBinding<T> : Binding<T>, IServerBinding
        where T : IServerBinding, new()
    {
        protected abstract FixedFieldMap<T> FixedFieldMap { get; }

        public override T LoadBinding(PropertyNode node) => BindingDeserializer.LoadBinding("ServerBinding", node.Value, this.FixedFieldMap);
    }
}
