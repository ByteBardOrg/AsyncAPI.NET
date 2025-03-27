namespace ByteBard.AsyncAPI.Bindings
{
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Readers;
    using ByteBard.AsyncAPI.Readers.ParseNodes;

    public abstract class ChannelBinding<T> : Binding<T>, IChannelBinding
        where T : IChannelBinding, new()
    {
        protected abstract FixedFieldMap<T> FixedFieldMap { get; }

        public override T LoadBinding(PropertyNode node) => BindingDeserializer.LoadBinding("ChannelBinding", node.Value, this.FixedFieldMap);
    }
}
