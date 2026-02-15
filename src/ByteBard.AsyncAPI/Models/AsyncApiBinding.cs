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

        public abstract void SerializeV2(IAsyncApiWriter writer);

        public abstract void SerializeV3(IAsyncApiWriter writer);

        public abstract void SerializeProperties(IAsyncApiWriter writer);
    }
}
