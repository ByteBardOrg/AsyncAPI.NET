namespace ByteBard.AsyncAPI.Bindings
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Writers;

    public abstract class AsyncApiBinding : IBinding
    {
        public abstract string BindingKey { get; }

        public IDictionary<string, IAsyncApiExtension> Extensions { get; set; } = new Dictionary<string, IAsyncApiExtension>();

        public string BindingVersion { get; set; }

        public void SerializeV2(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer);
        }

        public void SerializeV3(IAsyncApiWriter writer)
        {
            this.SerializeCore(writer);
        }

        private void SerializeCore(IAsyncApiWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.SerializeProperties(writer);
        }

        public abstract void SerializeProperties(IAsyncApiWriter writer);
    }
}
