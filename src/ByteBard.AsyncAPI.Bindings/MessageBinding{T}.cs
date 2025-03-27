namespace ByteBard.AsyncAPI.Bindings
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public abstract class MessageBinding<T> : Binding<T>, IMessageBinding
        where T : IMessageBinding, new()
    {
        protected abstract FixedFieldMap<T> FixedFieldMap { get; }

        public override T LoadBinding(PropertyNode node) => BindingDeserializer.LoadBinding("MessageBinding", node.Value, this.FixedFieldMap);
    }
}
